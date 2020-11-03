using API;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoginFrame : MonoBehaviour
{
    public InputField input_account;
    public InputField input_password;


    bool startServer = false;

    private void Start()
    {
    }
    public void ClickStartServer()
    {
        Singleton<SocketServer>.GetInstance().Start();
        startServer = true;
    }

    public void Update()
    {
        if(startServer)
            Singleton<SocketServer>.GetInstance().Update();
    }
    public void ClickStartConnectServer()
    {
        NetStateManager.GetInstance().startConnect();
    }

    public void ClickLogin()
    {
        if(string.IsNullOrEmpty( input_account.text))
        {
            Debug.LogError("请输入账号！");
            return;
        }

        InitService.GetInstance().onLoginRequst( input_account.text);
    }
  

}
