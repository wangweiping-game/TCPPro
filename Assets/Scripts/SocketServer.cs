using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using UnityEngine;
using API;

public class SocketServer :Singleton<SocketServer>
{
    // 创建一个和客户端通信的套接字
    static Socket socketwatch = null;
    //定义一个集合，存储客户端信息
    static Dictionary<string, Socket> clientConnectionItems = new Dictionary<string, Socket> { };
    static List<Operation> OperationsList = new List<Operation> ();
    private static MemoryStream memStream;
    private static BinaryReader reader;
    private static int maxSocketNum = 1;

    private const int MAX_READ = 8192;
    private static byte[] byteBuffer;
    private const int PROTOCOL_HEAD_LENGTH = 8;
    private const int PROTOCOL_HEAD_LENGTH_MASK = 0xFFFFFF;
    private const int PROTOCOL_HEAD_COMPRESS_MASK = 0x1000000;
    private static MemoryStream receiveStream = new MemoryStream();
    private static MemoryStream operationsStream = new MemoryStream();
    private static int serialNumber = 0;
    private Timer roomInfoTimer;
    private static bool notifyRoomInfoFlag = false;
    public  void Start()
    {
        byteBuffer = new byte[MAX_READ];
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);

        //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议）  
        socketwatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //服务端发送信息需要一个IP地址和端口号  
        IPAddress address = IPAddress.Parse("127.0.0.1");
        //将IP地址和端口号绑定到网络节点point上  
        IPEndPoint point = new IPEndPoint(address, 1234);
        //此端口专门用来监听的  

        //监听绑定的网络节点  
        socketwatch.Bind(point);

        //将套接字的监听队列长度限制为20  
        socketwatch.Listen(20);

        //负责监听客户端的线程:创建一个监听线程  
        Thread threadwatch = new Thread(watchconnecting);

        //将窗体线程设置为与后台同步，随着主线程结束而结束  
        threadwatch.IsBackground = true;

        //启动线程     
        threadwatch.Start();
        GameModel.serverStartFlag = true;

        roomInfoTimer = TimerManager.GetInstance().createTimer(0.1f, NotifyRoomInfo);
        roomInfoTimer.start();
        Debug.Log("*******服务器启动成功*******");
    }
    public void Update()
    {
        SyncOperationsPerFrame();//帧同步
    }
    void SyncOperationsPerFrame()
    {
        lock (OperationsList)
        {
            operationsStream.SetLength(0);
            operationsStream.Position = 0;
            ByteBuffer buffer;
            MessageNotifySyncOperations msg = new MessageNotifySyncOperations();
            if (OperationsList.Count > 0)
                msg.PlayerOperations.AddRange(OperationsList);
            Google.Protobuf.MessageExtensions.WriteTo(msg, operationsStream);
            buffer = createByteBuffer((ushort)MSG_CS.NotifySyncOperations, operationsStream);
            foreach (var client in clientConnectionItems)
            {
                client.Value.BeginSend(buffer.ToBytes(), 0, buffer.ToBytes().Length, SocketFlags.None, null, null);
            }
            OperationsList.Clear();
        }
        
    }
    static void NotifyRoomInfo()
    {
        if (notifyRoomInfoFlag)
            return;
        MessageNotifyRoomInfo msg = new MessageNotifyRoomInfo();
        msg.PlayerCount = clientConnectionItems.Count;
        //0：等待 1:开战中 
        if (clientConnectionItems.Count >= maxSocketNum)
        {
            msg.FightState = 1;
            notifyRoomInfoFlag = true;
        }
        else
            msg.FightState = 0;
        MemoryStream memory = new MemoryStream();
        Google.Protobuf.MessageExtensions.WriteTo(msg, memory);
        ByteBuffer buffer = createByteBuffer((ushort)MSG_CS.NotifyRoomInfo, memory);
        foreach (var client in clientConnectionItems)
        {
            client.Value.BeginSend(buffer.ToBytes(), 0, buffer.ToBytes().Length, SocketFlags.None, null, null);
        }
    }
    //监听客户端发来的请求  
    static void watchconnecting()
    {
        if (clientConnectionItems.Count >= maxSocketNum)
        {
            Debug.Log("房间人数达到上限");
            return;
        }

        Socket connection = null;

        //持续不断监听客户端发来的请求     
        while (true)
        {
            try
            {
                connection = socketwatch.Accept();
            }
            catch (Exception ex)
            {
                //提示套接字监听异常     
                Console.WriteLine(ex.Message);
                break;
            }

            //获取客户端的IP和端口号  
            IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
            int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

            //让客户显示"连接成功的"的信息  
            //string sendmsg = "连接服务端成功！\r\n" + "本地IP:" + clientIP + "，本地端口" + clientPort.ToString();
            //byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
            //connection.Send(arrSendMsg);

            //客户端网络结点号  
            string remoteEndPoint = connection.RemoteEndPoint.ToString();
            //显示与客户端连接情况
            Debug.Log("成功与" + remoteEndPoint + "客户端建立连接！");
            //添加客户端信息  
            clientConnectionItems.Add(remoteEndPoint, connection);

            //IPEndPoint netpoint = new IPEndPoint(clientIP,clientPort); 
            IPEndPoint netpoint = connection.RemoteEndPoint as IPEndPoint;

            //创建一个通信线程      
            ParameterizedThreadStart pts = new ParameterizedThreadStart(recv);
            Thread thread = new Thread(pts);
            //设置为后台线程，随着主线程退出而退出 
            thread.IsBackground = true;
            //启动线程     
            thread.Start(connection);
        }
    }

    /// <summary>
    /// 接收客户端发来的信息，客户端套接字对象
    /// </summary>
    /// <param name="socketclientpara"></param>    
    static void recv(object socketclientpara)
    {
        Socket socketServer = socketclientpara as Socket;

        while (true)
        {
            //System.Threading.Thread.Sleep(200/*new System.Random().Next(200)*/);
            //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度    
            try
            {
                int length = socketServer.Receive(byteBuffer);
                memStream.SetLength(0);
                memStream.Seek(0, SeekOrigin.End);
                memStream.Write(byteBuffer, 0, length);
                memStream.Seek(0, SeekOrigin.Begin);
                bool isFullMsg = false;

                while ((int)(memStream.Length - memStream.Position) >= PROTOCOL_HEAD_LENGTH)
                {
                    int totalLen = reader.ReadInt32();
                    int messageLen = totalLen & PROTOCOL_HEAD_LENGTH_MASK;
                    ushort msgId = reader.ReadUInt16();
                    UInt16 serialNumber = reader.ReadUInt16();
                    if ((int)(memStream.Length - memStream.Position) >= messageLen)
                    {
                        //Debug.Log("服务端收到请求：" + msgId);
                        isFullMsg = true;
                        byte[] data = reader.ReadBytes(messageLen);

                        receiveStream.SetLength(0);
                        receiveStream.Write(data, 0, data.Length);
                        receiveStream.Position = 0;
                        ByteBuffer buffer;
                        switch (msgId)
                        {
                            case (ushort)MSG_CS.ReqLogin:
                                MessageRequestLogin req = MessageRequestLogin.Parser.ParseFrom(receiveStream);
                                Debug.Log("Req login：account: " + req.Account + " password:" + req.Password);

                                MessageResponseLogin res = new MessageResponseLogin();
                                res.PlayerId = req.Account;
                                res.Token = req.Account + "666token";
                                receiveStream.SetLength(0);
                                Google.Protobuf.MessageExtensions.WriteTo(res, receiveStream);
                                buffer = createByteBuffer((ushort)MSG_CS.ResLogin, receiveStream);
                                socketServer.BeginSend(buffer.ToBytes(), 0, buffer.ToBytes().Length, SocketFlags.None, null, null);
                                break;
                            case (ushort)MSG_CS.ReqHeartBeat:
                                MessageRequestHeartBeat heartReq = MessageRequestHeartBeat.Parser.ParseFrom(receiveStream);
                                MessageResponseHeartBeat heartRes = new MessageResponseHeartBeat();
                                heartRes.RealTime = heartReq.RealTime;
                                heartRes.ServerTime = (ulong)DateTime.Now.Second;
                                Google.Protobuf.MessageExtensions.WriteTo(heartRes, receiveStream);
                                buffer = createByteBuffer((ushort)MSG_CS.ResHeartBeat, receiveStream);
                                socketServer.BeginSend(buffer.ToBytes(), 0, buffer.ToBytes().Length, SocketFlags.None, null, null);
                                break;
                            case (ushort)MSG_CS.NotifyClientOperations:
                                lock (OperationsList)
                                {
                                    MessageNotifyClientOperations ope = MessageNotifyClientOperations.Parser.ParseFrom(receiveStream);
                                    OperationsList.Add(ope.PlayerOperation);
                                    break;
                                }
                                    
                            default:
                                Debug.LogError("服务端没有此协议：" + msgId);
                                break;

                        }

                    }
                    else
                    {
                        memStream.Position = memStream.Position - PROTOCOL_HEAD_LENGTH;
                        break;
                    }
                }
                if (isFullMsg)
                {
                    int len = (int)(memStream.Length - memStream.Position);
                    if (len > 0)
                    {
                        //reader和memStream是关联的，所以必须先读取，再重置memStream
                        byte[] leftover = reader.ReadBytes(len);
                        memStream.SetLength(0);
                        memStream.Write(leftover, 0, leftover.Length);
                    }
                    else memStream.SetLength(0);
                }


            }
            catch (Exception ex)
            {
                clientConnectionItems.Remove(socketServer.RemoteEndPoint.ToString());

                Console.WriteLine("Client Count:" + clientConnectionItems.Count);

                //提示套接字监听异常  
                Console.WriteLine("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n");
                //关闭之前accept出来的和客户端进行通信的套接字 
                socketServer.Close();
                break;
            }
        }
    }


    private static ByteBuffer createByteBuffer(ushort msgId, MemoryStream stream)
    {
        serialNumber++;
        ByteBuffer buffer = new ByteBuffer();
        buffer.setMsgId(msgId);
        buffer.WriteInt(stream != null ? (int)stream.Length : 0);
        buffer.WriteShort(msgId);
        buffer.WriteShort((ushort)serialNumber);
        if (stream != null)
        {
            buffer.WriteStream(stream);
        }
        return buffer;
    }
    ///      
    /// 获取当前系统时间的方法    
    /// 当前时间     
    static DateTime GetCurrentTime()
    {
        DateTime currentTime = new DateTime();
        currentTime = DateTime.Now;
        return currentTime;
    }

}