using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloginState: BaseNetState
{
    public ReloginState()
    {
        name = NET_STATE.STATE_RELOGIN;
    }

    public override void enter()
    {
        Debug.Log("*********进入重登录状态*********");

        NetworkManager.GetInstance().onClear();

        switch(NetStateManager.GetInstance().loginState)
        {
            case LOGIN_STATE.STATE_NONE:
                break;
            case LOGIN_STATE.STATE_SAME_ACCOUNT:
                break;                
            case LOGIN_STATE.STATE_SWITCH_ACCOUNT:
                {
                    //Debug.LogWarning("切换帐号释放资源");
                    onReset();
                    GC.Collect();
                }
                break;
            case LOGIN_STATE.STATE_SWITCH_ACCOUNT_LOGIN:
                {
                    NetStateManager.GetInstance().onReset();
                }
                break;
        }
        NetStateManager.GetInstance().loginState = LOGIN_STATE.STATE_NONE;
    }

    void onMessageOkHandle()
    {
        onReset();
        //强制垃圾回收
        GC.Collect();
    }

    void onReset()
    {
        TimerManager.GetInstance().StopAllTimer();

        Singleton<GameModel>.GetInstance().onClear();
     
        NetStateManager.GetInstance().onReset();
    }

}
