using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;

public class ConnectState: BaseNetState
{
    Timer timeOut;
    public ConnectState()
    {
        name = NET_STATE.STATE_CONNECT;
    }

    public override void enter()
    {
        Debug.Log("*********开始连接服务器*********");
        if (NetworkManager.GetInstance().IsConnect)
        {
            handleConnectSucc();
            return;
        }
        NetworkManager.GetInstance().addEvent(NET_EVENT.CONNECT_SUCC, handleConnectSucc);
        NetworkManager.GetInstance().addEvent(NET_EVENT.CONNECT_FAIL, handleConnectFail);

        timeOut = TimerManager.GetInstance().createTimer(10f, closeConnect);
        timeOut.start();

        startConnect();
    }

    void handleConnectSucc(EventObject e = null)
    {
        if(null != timeOut)
        {
            timeOut.onDispose();
            timeOut = null;
        }
        sendLogin();
        NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_RUN);
    }

    protected override void handleConnectFail(EventObject e)
    {
        base.handleConnectFail(e);
        closeConnect();
    }

    //发送登录消息
    void sendLogin()
    {
        //InitService.GetInstance().onLoginRequst();
    }


    void closeConnect()
    {
        if (null != timeOut)
        {
            timeOut.onDispose();
            timeOut = null;
        }
        NetworkManager.GetInstance().closeConnect();
        NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_NONE);
    }

    public override void exit()
    {
        if (null != timeOut)
        {
            timeOut.onDispose();
            timeOut = null;
        }
        NetworkManager.GetInstance().removeEvent(NET_EVENT.CONNECT_FAIL, handleConnectFail);
        NetworkManager.GetInstance().removeEvent(NET_EVENT.CONNECT_SUCC, handleConnectSucc);
    }
}
