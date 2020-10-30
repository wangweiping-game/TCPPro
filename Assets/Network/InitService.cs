using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using API;
using Google.Protobuf;

public class InitService :Singleton<InitService>
{
    public InitService()
    {
        init();
    }

    void init()
    {
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.ResLogin, onLoginResponse);
    }
    /// <summary>
    /// 发送心跳
    /// </summary>
    public void sendHeartBeat()
    {
        MessageRequestHeartBeat request = new MessageRequestHeartBeat();
        request.RealTime = (ulong)(Time.realtimeSinceStartup * 1000);
        NetworkManager.GetInstance().SendMessage(MSG_CS.ReqHeartBeat, request);
    }

    /// <summary>
    /// 请求登录
    /// </summary>
    public void onLoginRequst()
    {
        MessageRequestLogin login = new MessageRequestLogin();
        login.Account = "wwp";
        login.Password = "123456";
        NetworkManager.GetInstance().SendMessage(MSG_CS.ReqLogin, login);
    }

    void onLoginResponse(MemoryStream stream)
    {
        MessageResponseLogin res = MessageResponseLogin.Parser.ParseFrom(stream);
        Singleton<GameModel>.GetInstance().Token = res.Token;
        Debug.Log("Res login：" + res.Token);
    }

}
