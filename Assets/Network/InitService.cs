using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sy;
using System.IO;
using ProtoBuf;

public class InitService :Singleton<InitService>
{
    public InitService()
    {
        init();
    }

    void init()
    {
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.MSG_CS_RESPONSE_LOGIN_S, onLoginResponse);
    }
    /// <summary>
    /// 发送心跳
    /// </summary>
    public void sendHeartBeat()
    {
        MessageRequestHeartBeat request = new MessageRequestHeartBeat();
        request.millisec = (ulong)(Time.realtimeSinceStartup * 1000);
        NetworkManager.GetInstance().SendMessage<MessageRequestHeartBeat>(MSG_CS.MSG_CS_REQUEST_HEART_BEAT, request);
    }

    /// <summary>
    /// 请求登录
    /// </summary>
    public void onLoginRequst()
    {
        MessageRequestLogin login = new MessageRequestLogin();
        login.account = "wwp";
        login.device_id = SystemInfo.deviceUniqueIdentifier;
        NetworkManager.GetInstance().SendMessage<MessageRequestLogin>(MSG_CS.MSG_CS_REQUEST_LOGIN_C, login);
    }

    void onLoginResponse(MemoryStream stream)
    {
        MessageResponseLogin res = Serializer.Deserialize<MessageResponseLogin>(stream);
        Singleton<GameModel>.GetInstance().Token = res.token;
        Debug.Log("Res login：" + res.token);
    }

}
