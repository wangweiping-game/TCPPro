using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : MonoBehaviour
{
    private static Game instance;

    public static Game GetInstance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        InitService.GetInstance().init();
        NetStateManager.GetInstance().Start();
        UIManager.GetInstance().Start();

        UIManager.GetInstance().ShowPanel(ID_PANEL.LoginPanel);
    }

    void Update()
    {
        if(GameModel.serverStartFlag)
        {
            SocketServer.GetInstance().Update();
        }
    }

    private void LateUpdate()
    {
        NetStateManager.GetInstance().Update();
        TimerManager.GetInstance().Update(Time.deltaTime);
    }
    void OnApplicationQuit()
    {
        NetworkManager.GetInstance().closeConnect();
    }

    public void StartCoroutineS(IEnumerator func)
    {
        StartCoroutine(func);
    }

    public IEnumerator createRoom(string url, bool isHeart = false)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(GameModel.serverUrl + url);
        yield return webRequest.SendWebRequest();

        if (webRequest.isHttpError || webRequest.isNetworkError)
            Debug.Log(webRequest.error);
        else
        {
            if (!isHeart)
            {
                Debug.Log(webRequest.downloadHandler.text);
                analyDownLoadText(url, webRequest.downloadHandler.text);
            }
        }
    }
    void analyDownLoadText(string url, string str)
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
                GameModel.roomServerIp = Util.getSelfIp();
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
        yield return new WaitForSeconds(0.5f);
        NetStateManager.GetInstance().startConnect();
        yield return new WaitForSeconds(0.5f);
        UIManager.GetInstance().ShowPanel(ID_PANEL.WaitPanel, true);
    }
    IEnumerator joinRoomCallBack()
    {
        showPrompt("成功加入房间！");
        NetStateManager.GetInstance().startConnect();
        yield return new WaitForSeconds(2);
        UIManager.GetInstance().ShowPanel(ID_PANEL.WaitPanel, false);
    }
    public void showPrompt(string msg)
    {
        UIManager.GetInstance().ShowPanel(ID_PANEL.PromptPanel, msg);
    }

   

}
