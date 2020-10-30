using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;

public class SocketClient
{
    private Socket client;
    private MemoryStream memStream;
    private BinaryReader reader;

    private const int MAX_READ = 8192;
    private byte[] byteBuffer;
    private const int PROTOCOL_HEAD_LENGTH = 8;
    private const int PROTOCOL_HEAD_LENGTH_MASK = 0xFFFFFF;
    private const int PROTOCOL_HEAD_COMPRESS_MASK = 0x1000000;

    AsyncCallback connectCallBack;
    AsyncCallback readCallBack;
    AsyncCallback writeCallBack;
    
    public SocketClient()
    {
        byteBuffer = new byte[MAX_READ];
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
        readCallBack = new AsyncCallback(OnRead);
        writeCallBack = new AsyncCallback(OnWrite);
        connectCallBack = new AsyncCallback(OnConnect);
    }

    // 连接服务器
	public void ConnectServer(string host, int port)
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        client.NoDelay = true;//关闭Nagle算法
        try
        {
            client.BeginConnect(host, port, connectCallBack, client);
        }
        catch (SocketException ex)
        {
            NetworkManager.GetInstance().handleConnectFail(ex.SocketErrorCode);
        }
    }

    // 连接服务器回调   
    void OnConnect(IAsyncResult asr)
    {
        Socket sk = (Socket)asr.AsyncState;
        if (sk != client) 
            return;
        try
        {
            sk.EndConnect(asr);
            NetworkManager.GetInstance().pushEvent(NET_EVENT.CONNECT_SUCC);
            sk.BeginReceive(byteBuffer, 0, MAX_READ, SocketFlags.None, readCallBack, null);
        }
        catch (SocketException ex)
        {
            Debug.LogWarning("连接处理失败：" + ex.SocketErrorCode);
            NetworkManager.GetInstance().pushEvent(NET_EVENT.CONNECT_FAIL,ex.SocketErrorCode);
        }
    }
    
    // 写数据
    public void WriteMessage(byte[] message)
    {
        if (client != null && client.Connected)
        {
            client.BeginSend(message, 0, message.Length, SocketFlags.None, writeCallBack, null);
        }
        else
        {
            NetworkManager.GetInstance().handleConnectFail(SocketError.NotConnected);
        }
    }
    
    // 读取消息
    void OnRead(IAsyncResult asr)
    {
        if (client == null || !client.Connected) return;
        try
        {
            int bytesRead = client.EndReceive(asr);
            if (bytesRead > 0)
            {
                OnReceive(bytesRead);
                client.BeginReceive(byteBuffer, 0, MAX_READ, SocketFlags.None, readCallBack, null);
            }
            else
            {
                Debug.LogError("连接处理失败");
                NetworkManager.GetInstance().pushEvent(NET_EVENT.CONNECT_FAIL, SocketError.NotConnected);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogWarning("读取失败：" + ex.SocketErrorCode);
            NetworkManager.GetInstance().pushEvent(NET_EVENT.CONNECT_FAIL,ex.SocketErrorCode);            
        }
    }
    
    // 链接写入数据流回调
    void OnWrite(IAsyncResult result)
    {
        if (client == null || !client.Connected) return;
        try
        {
            int len = client.EndSend(result);
        }
        catch(SocketException ex)
        {
            Debug.LogWarning("写入失败：" + ex.SocketErrorCode);
            NetworkManager.GetInstance().pushEvent(NET_EVENT.CONNECT_FAIL,ex.SocketErrorCode);
        }
    }
    
    // 接收到消息,解析协议
    void OnReceive(int length)
    {
        memStream.Seek(0, SeekOrigin.End);
        memStream.Write(byteBuffer, 0, length);
        memStream.Seek(0, SeekOrigin.Begin);
        bool isFullMsg = false;
        while (remainingByteLen() >= PROTOCOL_HEAD_LENGTH)
        {
            int totalLen = reader.ReadInt32();
            int messageLen = totalLen & PROTOCOL_HEAD_LENGTH_MASK;            
            ushort msgId = reader.ReadUInt16();
            UInt16 serialNumber = reader.ReadUInt16();
            if (remainingByteLen() >= messageLen)
            {
                isFullMsg = true;
                byte[] data = reader.ReadBytes(messageLen);

                Debug.Log("客户端收到协议："+ msgId);
                //断线重连协议号校验
                if (NetworkManager.GetInstance().handledSerialNumber == UInt16.MaxValue)
                {
                    NetworkManager.GetInstance().onClearSerialNumber();
                }                    
				if(0 == serialNumber || NetworkManager.GetInstance().handledSerialNumber < serialNumber)
                {
                    NetworkManager.GetInstance().pushMessage(msgId, data);
                    NetworkManager.GetInstance().handledSerialNumber = serialNumber;                    
                }
                else
                {
                    Debug.LogError("服务器发送重复的协议：" + msgId +  " 序列号：" + serialNumber );
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
            int len = remainingByteLen();            
            if (len > 0)
            {
                //reader和memStream是关联的，所以必须先读取，再重置memStream
                byte[] leftover = reader.ReadBytes(len);
                memStream.SetLength(0);
                memStream.Write(leftover, 0, leftover.Length);
            }
            else 
                memStream.SetLength(0);
        }
    }

    // 剩余的字节长度
    private int remainingByteLen()
    {
        return (int)(memStream.Length - memStream.Position);
    }
    public bool isConnected
    {
       get
        {
            if (null != client)
                return client.Connected;
            return false;
        }
    }

    // 关闭连接
    public void close()
    {
        memStream.SetLength(0);
        if (null == client) return;

        try
        {
            if (client.Connected)
                client.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException ex)
        {
            Debug.LogError(ex);
        }            

        client.Close();
        client = null;
    }
    
}
