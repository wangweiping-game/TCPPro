using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;

public class RunState: BaseNetState
{
    public RunState()
    {
        name = NET_STATE.STATE_RUN;
    }

    public override void enter()
    {
        Debug.Log("*********进入运行状态*********");
        NetworkManager.GetInstance().addEvent(NET_EVENT.CONNECT_FAIL, handleConnectFail);
        //startHeartBeat();
    }

    protected override void handleConnectFail(EventObject e)
    {
        base.handleConnectFail(e);
        NetStateManager.GetInstance().changeNetState(NET_STATE.STATE_RECONNECT);
    }

    public override void update()
    {
        NetworkManager.GetInstance().handleReceive();
        NetworkManager.GetInstance().handleEvent();        
        NetworkManager.GetInstance().handleSend();
    }

    public override void exit()
    {
        NetworkManager.GetInstance().removeEvent(NET_EVENT.CONNECT_FAIL, handleConnectFail);
        stopHeartBeat();
    }
}
