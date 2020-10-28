using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using sy;

public class NetStateManager : Singleton<NetStateManager>
{
    GameStateMachine netStateMachine;
    public string loginState = LOGIN_STATE.STATE_NONE;

    public void Start()
    {        
        initState();
    }

    public void Update()
    {        
        if (null != netStateMachine) 
            netStateMachine.update();        
    }

    void initState()
    {
        netStateMachine = new GameStateMachine();
        GameState state = new PreConnectState();
        netStateMachine.addState(state);
        state = new ConnectState();
        netStateMachine.addState(state);
        state = new ReconnectState();
        netStateMachine.addState(state);
        state = new ReloginState();
        netStateMachine.addState(state);
        state = new RunState();
        netStateMachine.addState(state);
        state = new QuitState();
        netStateMachine.addState(state);
    }

    public void startConnect()
    {
        netStateMachine.changeState(NET_STATE.STATE_CONNECT);
    }

    public void changeNetState(string name)
    {
        netStateMachine.changeState(name);
    }


    void handleRelogin(MemoryStream ms)
    {
        loginState = LOGIN_STATE.STATE_SAME_ACCOUNT;
        netStateMachine.changeState(NET_STATE.STATE_RELOGIN);
    }

    public void onSwitchAccout(bool isLogin = false)
    {
        if(isLogin) loginState = LOGIN_STATE.STATE_SWITCH_ACCOUNT_LOGIN;
        else loginState = LOGIN_STATE.STATE_SWITCH_ACCOUNT;
        netStateMachine.changeState(NET_STATE.STATE_RELOGIN);
        loginState = LOGIN_STATE.STATE_NONE;
    }

    public void onDestroy()
    {
        NetworkManager.GetInstance().onDestroy();
    }

    public void onReset()
    {
        loginState = LOGIN_STATE.STATE_NONE;
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.MSG_CS_NOTIFY_RELOGIN, handleRelogin);
    }
}

public struct NET_STATE
{
    public const string STATE_NONE = "state_none";
    public const string STATE_PRECONNECT = "state_preconnect";
    public const string STATE_CONNECT = "state_connect";
    public const string STATE_RECONNECT = "state_reconnect";
    public const string STATE_RELOGIN = "state_relogin";
    public const string STATE_RUN = "state_run";
    public const string STATE_QUIT = "state_quit";
}

public struct LOGIN_STATE
{
    public const string STATE_NONE = "state_none";
    public const string STATE_SAME_ACCOUNT = "state_same_account";
    public const string STATE_SWITCH_ACCOUNT = "state_switch_account";
    public const string STATE_SWITCH_ACCOUNT_LOGIN = "state_switch_account_login";
}