using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class ByteBuffer
{
    MemoryStream stream = null;
    BinaryWriter writer = null;

    ushort msgId = 0;
    public ByteBuffer()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
    }

    public void setMsgId(ushort id)
    {
        msgId = id;
    }

    public ushort getMsgId()
    {
        return msgId;
    }

    public void Close()
    {
        if (writer != null)
            writer.Close();
        stream.Dispose();
        stream.Close();
        writer = null;
        stream = null;
    }

    public void WriteByte(int v)
    {
        writer.Write((byte)v);
    }

    public void WriteInt(int v)
    {
        writer.Write((int)v);
    }

    public void WriteShort(ushort v)
    {
        writer.Write((ushort)v);
    }

    public void WriteStream(MemoryStream memStream)
    {
        memStream.WriteTo(stream);
    }

    public byte[] ToBytes()
    {
        writer.Flush();
        return stream.ToArray();
    }

    public void Flush()
    {
        writer.Flush();
    }
}
