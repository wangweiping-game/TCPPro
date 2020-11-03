using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using API;

public class BaseNetState:GameState
{
    private Timer heartBeatTimer;
    private float runTime = 0;

    public override void enter(){}

    public override void update()
    {
        NetworkManager.GetInstance().handleEvent();
    }

    public override void exit() { }

    protected void startConnect()
    {
        NetworkManager.GetInstance().startConnect("127.0.0.1", 1234);
    }

    protected virtual void handleConnectFail(EventObject e)
    {
        SocketError error = (SocketError)e.obj;
        Debug.LogError("连接失败:" + error);
        switch (error)
        {
            case SocketError.HostNotFound:
                break;
            case SocketError.TimedOut:
                break;
            case SocketError.NetworkUnreachable:
                break;
            case SocketError.ConnectionReset:
                break;
            case SocketError.IsConnected:
                break;
            case SocketError.ConnectionRefused:
                break;
            case SocketError.ConnectionAborted:
                break;
            case SocketError.NotConnected:
                break;
            case SocketError.Shutdown:
                break;
            default:
                break;
        }
    }

    protected void sendHeartBeat()
    {
        InitService.GetInstance().sendHeartBeat();
    }

    protected void startHeartBeat()
    {
        if(null != heartBeatTimer)
        {
            heartBeatTimer.onDispose();
            heartBeatTimer = null;
        }

        NetworkManager.GetInstance().addEvent(NET_EVENT.HANDLE_START, handleMsgStart);
        //这个协议处理不需要remove
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.ResHeartBeat, handleHeartBeatResponse);

        heartBeatTimer = TimerManager.GetInstance().createTimer(10f, handleHeartBeat);
        heartBeatTimer.start();
        runTime = 0;
    }

    void handleMsgStart(EventObject e)
    {
        runTime = 0;
    }

    void handleHeartBeatResponse(MemoryStream ms)
    {
        runTime = 0;
    }

    void handleHeartBeat()
    {
        runTime += 10f;
        if(runTime >= 30)
        {
            //收到心跳时间间隔超过30S,进入断线重连状态
            NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_RECONNECT);
        }
        else
            sendHeartBeat();
    }

    protected void stopHeartBeat()
    {
        if (null != heartBeatTimer)
        {
            heartBeatTimer.onDispose();
            heartBeatTimer = null;
        }
        NetworkManager.GetInstance().removeEvent(NET_EVENT.HANDLE_START, handleMsgStart);
    }
}
