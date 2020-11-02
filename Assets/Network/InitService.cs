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

    public void NotifyClientOperations(float xOffset, float zOffset)
    {
        MessageNotifyClientOperations msg = new MessageNotifyClientOperations();
        Operation info = new Operation();
        info.PlayerId = Singleton<GameModel>.GetInstance().PlayerID;
        info.XOffset = xOffset;
        info.ZOffset = zOffset;
        msg.PlayerOperation = info;
        NetworkManager.GetInstance().SendMessage(MSG_CS.NotifyClientOperations, msg);
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
    public void onLoginRequst(string account,string password = "")
    {
        MessageRequestLogin login = new MessageRequestLogin();
        login.Account = account;
        login.Password = "123456";
        NetworkManager.GetInstance().SendMessage(MSG_CS.ReqLogin, login);
    }

    void onLoginResponse(MemoryStream stream)
    {
        MessageResponseLogin res = MessageResponseLogin.Parser.ParseFrom(stream);
        Singleton<GameModel>.GetInstance().Token = res.Token;
        Singleton<GameModel>.GetInstance().PlayerID = res.PlayerId;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
        Debug.Log("Res login：" + res.Token);
    }

}
