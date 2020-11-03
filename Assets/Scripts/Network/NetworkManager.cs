using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using API;
using Google.Protobuf;

public class NetworkManager : EventDispatcher
{
    private static NetworkManager instance;
    private SocketClient client = new SocketClient();
    private Queue<KeyValuePair<int, byte[]>> receiveQueue = new Queue<KeyValuePair<int, byte[]>>();
    private Queue<ByteBuffer> sendQueue = new Queue<ByteBuffer>();
    private Queue<KeyValuePair<string, object>> eventQueue = new Queue<KeyValuePair<string, object>>();
    private Dictionary<int, Action<MemoryStream>> handleList = new Dictionary<int, Action<MemoryStream>>();
    private MemoryStream requestStream = new MemoryStream();
    private MemoryStream receiveStream = new MemoryStream();
    //累计处理过的协议序列号，用于断线重连后的服务器发存储协议的校验判断
    public Int32 handledSerialNumber = -1;
    public static NetworkManager GetInstance()
    {
        if (null == instance)
            instance = new NetworkManager();
        return instance;
    }

    public bool IsConnect
    {
        get { return client.isConnected; }
    }

    //网络完整包数据放入队列
    public void pushMessage(int msgId, byte[] data)
    {
        receiveQueue.Enqueue(new KeyValuePair<int, byte[]>(msgId, data));
    }

    //网络异步回调事件放入队列,避免在非主线程中执行
    public void pushEvent(string key, object param = null)
    {
        eventQueue.Enqueue(new KeyValuePair<string, object>(key, param));
    }

    // 处理队列中的网络事件
    public void handleEvent()
    {
        while (eventQueue.Count > 0)
        {
            KeyValuePair<string, object> netEvent = eventQueue.Dequeue();
            dispatchEvent(netEvent.Key, netEvent.Value);
        }
    }

    // 添加每个协议对应的处理方法
    public void AddHandle(int msgId, Action<MemoryStream> handle)
    {
        if (!handleList.ContainsKey(msgId))
        {
            handleList[msgId] = handle;
        }
    }

    public void RemoveHandle(int msgId)
    {
        if (handleList.ContainsKey(msgId))
        {
            handleList.Remove(msgId);
        }
    }

    // 开启网络连接
    public void startConnect(string ip, int port)
    {
        client.ConnectServer(ip, port);
    }

    // 处理接收回来的消息
    public void handleReceive()
    {
        while (receiveQueue.Count > 0)
        {
            KeyValuePair<int, byte[]> msg = receiveQueue.Dequeue();
            dispatchEvent(NET_EVENT.HANDLE_START, msg.Key);
            handleMessage(msg.Key, msg.Value);
        }
    }

    // 向服务器发送队列里的消息
    public void handleSend()
    {
        if (!IsConnect)
            return;
  
        while (sendQueue.Count > 0)
        {
            ByteBuffer buffer = sendQueue.Dequeue();
            dispatchEvent(NET_EVENT.SEND_START, buffer.getMsgId());
            client.WriteMessage(buffer.ToBytes());
            buffer.Close();
        }
    }

    // 处理协议数据
    private void handleMessage(int key, byte[] buffer)
    {
        if (handleList.ContainsKey(key))
        {
            Action<MemoryStream> handle = handleList[key];
            if (handle != null)
            {
                if (buffer != null)
                {
                    receiveStream.SetLength(0);
                    receiveStream.Write(buffer, 0, buffer.Length);
                    receiveStream.Position = 0;
                    handle(receiveStream);
                }
                else
                {
                    handle(null);
                }
            }
        }
        else
        {
            dispatchEvent(NET_EVENT.HANDLE_NONE, key);
        }
        
    }

    public void handleConnectFail(SocketError error)
    {
        dispatchEvent(NET_EVENT.CONNECT_FAIL, error);
    }

    public void SendMessage(ushort msgId)
    {
        sendMsg((ushort)msgId, null);
    }

    public void SendMessage(MSG_CS msgId, IMessage request)
    {
        requestStream.SetLength(0);
        MessageExtensions.WriteTo(request, requestStream);
        sendMsg((ushort)msgId, requestStream);
    }
    public void SendMessageSync(MSG_CS msgId, IMessage request)
    {
        requestStream.SetLength(0);
        MessageExtensions.WriteTo(request, requestStream);
        ByteBuffer buffer = createByteBuffer((ushort)msgId, requestStream);
        client.WriteMessage(buffer.ToBytes());
        buffer.Close();
    }

    void sendMsg(ushort msgId, MemoryStream stream)
    {
        ByteBuffer buffer = createByteBuffer(msgId, stream);
        sendQueue.Enqueue(buffer);
    }

    private ByteBuffer createByteBuffer(ushort msgId, MemoryStream stream)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.setMsgId(msgId);
        buffer.WriteInt(stream != null ? (int)stream.Length : 0);
        buffer.WriteShort(msgId);
        buffer.WriteShort(0);
        if (stream != null)
        {
            buffer.WriteStream(stream);
        }
        return buffer;
    }

    public void closeConnect()
    {
        eventQueue.Clear();
        client.close();
        dispatchEvent(NET_EVENT.CONNECT_CLOSE);
    }

    public override void onClear()
    {
        onClearSerialNumber();
        sendQueue.Clear();
        receiveQueue.Clear();
        eventQueue.Clear();
        handleList.Clear();
        closeConnect();
    }

    public void onDestroy()
    {
        client.close();
    }

    public void onClearSerialNumber()
    {
        handledSerialNumber = -1;
    }
}

public struct NET_EVENT
{
    public const string CONNECT_START = "connect_start";
    public const string CONNECT_SUCC = "connect_succ";
    public const string CONNECT_FAIL = "connect_fail";
    public const string CONNECT_CLOSE = "connect_close";

    // 协议开始发送
    public const string SEND_START = "send_start";

    // 协议没有对应处理方法：参数为协议号
    public const string HANDLE_NONE = "handle_none";
    public const string HANDLE_START = "handle_start";
    public const string HANDLE_END = "handle_end";

}