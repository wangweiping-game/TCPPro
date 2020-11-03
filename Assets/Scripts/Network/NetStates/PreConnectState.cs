using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreConnectState : BaseNetState
{
    public PreConnectState()
    {
        name = NET_STATE.STATE_PRECONNECT;
    }

    public override void enter()
    {
        Debug.Log("*********进入预连接状态*********");
        NetworkManager.GetInstance().onClearSerialNumber();
        NetworkManager.GetInstance().closeConnect();
    }


}
