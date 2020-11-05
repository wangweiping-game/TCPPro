using API;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginFrame : MonoBehaviour
{
    public InputField input_account;
    public InputField input_password;
    public Animator promptAni;
    public Text promptText;
    public GameObject WaitFrame;
    public Text playerNumText;
    public GameObject startFigthBtn;
    private Timer heartTimer;

    private void Start()
    {
        //Debug.Log(getSelfIp());
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.NotifyRoomInfo, UpdateRoomInfo);
        heartTimer = TimerManager.GetInstance().createTimer(0.1f, heartEvent);
        startFigthBtn.SetActive(false);
    }
    public void ClickStartServer()
    {
        SocketServer.GetInstance().Start();
    }

    public void Update()
    {
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
    public void onClickStartFight()
    {
        InitService.GetInstance().ReqStartFight();
    }
  
    public void onClickCreateRoomBtn()
    {

        StartCoroutine(createRoom("create"));
    }

    public void onClickJoinRoomBtn()
    {
        StartCoroutine(createRoom("get"));
    }
    void heartEvent()
    {
        StartCoroutine(createRoom("create", true));
    }
    IEnumerator createRoom(string url,bool isHeart = false)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(GameModel.serverUrl + url);
        yield return webRequest.SendWebRequest();

        if (webRequest.isHttpError || webRequest.isNetworkError)
            Debug.Log(webRequest.error);
        else
        {
            if(!isHeart)
            {
                Debug.Log(webRequest.downloadHandler.text);
                analyDownLoadText(url, webRequest.downloadHandler.text);
            }
        }
    }
    void analyDownLoadText(string url,string str)
    {
        Dictionary<string, object> configDic = AGMiniJSON.Json.Deserialize(str) as Dictionary<string, object>;
        if (!configDic.ContainsKey("code"))
        {
            showPrompt("服务端下发数据错误！");
            return;
        }
        int code = int.Parse(configDic["code"].ToString());
        if (code != 0)
        {
            showPrompt(configDic["msg"].ToString());
            return;
        }
        switch (url)
        {
            case "get":
                GameModel.roomServerIp = configDic["val"].ToString();
                StartCoroutine(joinRoomCallBack());
                break;
            case "create":
                GameModel.roomServerIp = getSelfIp();
                StartCoroutine(createRoomCallBack());
                break;
            default:
                Debug.LogError("未处理！");
                break;

        }
    }
    IEnumerator createRoomCallBack()
    {
        SocketServer.GetInstance().Start();
        showPrompt("房间创建成功，请等待其他玩家的加入！");
        heartTimer.start();
        yield return new WaitForSeconds(0.5f);
        NetStateManager.GetInstance().startConnect();
        yield return new WaitForSeconds(0.5f);

        WaitFrame.SetActive(true);
        startFigthBtn.SetActive(true);
    }
    IEnumerator joinRoomCallBack()
    {
        showPrompt("成功加入房间！");
        NetStateManager.GetInstance().startConnect();
        yield return new WaitForSeconds(2);
        WaitFrame.SetActive(true);
    }
    public void showPrompt(string msg)
    {
        promptAni.enabled = true;
        promptAni.Play("PromptAnimation",0,0f);
        promptText.text = msg;
    }

    void UpdateRoomInfo(MemoryStream ms)
    {
        MessageNotifyRoomInfo info = MessageNotifyRoomInfo.Parser.ParseFrom(ms);
        playerNumText.text = info.PlayerInfoArray.Count.ToString();
        
        if (info.FightState == 1)
        {
            Singleton<GameModel>.GetInstance().UID = info.SelfUid;
            Singleton<GameModel>.GetInstance().roomInfoList.Clear();
            Singleton<GameModel>.GetInstance().roomInfoList.AddRange(info.PlayerInfoArray);
            heartTimer.stop();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
        }
    }
    string getSelfIp()
    {
        ///获取本地的IP地址
        string AddressIP = string.Empty;
        foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
            {
                AddressIP = _IPAddress.ToString();
            }
        }
        return AddressIP;

    }
}
