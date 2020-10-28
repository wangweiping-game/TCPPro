using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProtoBuf;
using sy;

public class ReconnectState: BaseNetState
{
    private float reConnectStartTime = 0;
    private float reConnectTime = 0;
    //每隔一定时间连接一次
    private Timer connectTimer;
    private Timer receiveTokenTimer;
    public ReconnectState()
    {
        name = NET_STATE.STATE_RECONNECT;
        reConnectTime = 60f; 
    }

    public override void enter()
    {
        Debug.Log("*********进入断线重连状态*********");
        //停止心跳
        stopHeartBeat();

        NetworkManager.GetInstance().addEvent(NET_EVENT.CONNECT_SUCC, handleConnectSucc);
        NetworkManager.GetInstance().addEvent(NET_EVENT.CONNECT_FAIL, handleConnectFail);
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.MSG_CS_REPORT_TOKEN_S, handleToken);

        reConnectStartTime = Time.realtimeSinceStartup;
        NetworkManager.GetInstance().closeConnect();
        if (null != connectTimer)
        {
            connectTimer.onDispose();
            connectTimer = null;
        }
        connectTimer = TimerManager.GetInstance().createTimer(5f, onConnect);
        connectTimer.start();
    }

    void handleConnectSucc(EventObject e)
    {
        Debug.Log("断线重连成功！");
        if (null != connectTimer) 
            connectTimer.stop();
        sendToken();
    }

    protected override void handleConnectFail(EventObject e)
    {
        base.handleConnectFail(e);
        //收到错误码，继续重连
        if (null != receiveTokenTimer)
        {
            receiveTokenTimer.onDispose();
            receiveTokenTimer = null;
        }
        NetworkManager.GetInstance().closeConnect();
        handleReconnect();
    }

    void handleReconnect()
    {        
        if (null != connectTimer) 
            connectTimer.reStart();
    }

    void onConnect()
    {
        //重连时间超出配置时间，停止重连，进入重登录状态
        if((Time.realtimeSinceStartup - reConnectStartTime) >= reConnectTime)
        {
            NetStateManager.GetInstance().loginState = LOGIN_STATE.STATE_NONE;
            Debug.LogError("重连超时,重新登录！");
            NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_RELOGIN);
            return;
        }
        NetworkManager.GetInstance().closeConnect();
        startConnect();
    }

    //发送Token校验
    void sendToken()
    {
        if (string.IsNullOrEmpty(Singleton<GameModel>.GetInstance().Token.Trim()))
        {
            Debug.LogError("Token为空,进入重登录");
            NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_RELOGIN);
            return;
        }
        MessageReportToken token = new MessageReportToken();
        token.token = Singleton<GameModel>.GetInstance().Token.Trim();
        NetworkManager.GetInstance().SendMessageSync<MessageReportToken>(MSG_CS.MSG_CS_REPORT_TOKEN_C, token);
        //这里还需要一个计时器，如果一定时间收不到token回复，则继续连接
        receiveTokenTimer = TimerManager.GetInstance().createTimer(5f, handleTokenFail);
    }

    void handleTokenFail()
    {
        NetworkManager.GetInstance().closeConnect();
        handleReconnect();
        if (null != receiveTokenTimer)
        {
            receiveTokenTimer.onDispose();
            receiveTokenTimer = null;
        }
    }

    void handleToken(MemoryStream ms)
    {
        MessageReportTokenS response = Serializer.Deserialize<MessageReportTokenS>(ms);
        if(0 == response.error_id)
        {
            //正常返回,进入运行状态
            NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_RUN);
        }
        else
        {
            Debug.LogError("收到错误Token,进入重登录");
            NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_CONNECT);
        }
    }

    public override void update()
    {
        //目的是接收Token协议
        NetworkManager.GetInstance().handleReceive();
        NetworkManager.GetInstance().handleEvent();
    }

    public override void exit()
    {
        if(null != connectTimer)
        {
            connectTimer.onDispose();
            connectTimer = null;
        }
        if (null != receiveTokenTimer)
        {
            receiveTokenTimer.onDispose();
            receiveTokenTimer = null;
        }
        
        NetworkManager.GetInstance().removeEvent(NET_EVENT.CONNECT_SUCC, handleConnectSucc);
        NetworkManager.GetInstance().removeEvent(NET_EVENT.CONNECT_FAIL, handleConnectFail);
        NetworkManager.GetInstance().RemoveHandle((int)MSG_CS.MSG_CS_REPORT_TOKEN_S);
    }
}
