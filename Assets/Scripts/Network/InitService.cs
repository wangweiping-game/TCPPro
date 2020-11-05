using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using API;
using Google.Protobuf;

public class InitService :Singleton<InitService>
{
    public void init()
    {
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.ResLogin, onLoginResponse);
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.NotifySyncOperations, NotifySynOperations);
    }
    //客户端发送指令
    public void NotifyClientOperations(Command type,string param)
    {
        MessageNotifyClientOperations msg = new MessageNotifyClientOperations();
        Operation info = new Operation();
        info.CommandType = type;
        info.Data = param;
        msg.PlayerOperation = info;
        NetworkManager.GetInstance().SendMessage(MSG_CS.NotifyClientOperations, msg);
    }
    /// <summary>
    /// 服务端下发帧同步命令集
    /// </summary>
    /// <param name="stream"></param>
    public void NotifySynOperations(MemoryStream stream)
    {
        MessageNotifySyncOperations res = MessageNotifySyncOperations.Parser.ParseFrom(stream);
        if(res.PlayerOperations.Count > 0)
        {
            List<Operation> opes = new List<Operation>(res.PlayerOperations);
            CommandManager.GetInstance().doCommand(opes);
        }

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
