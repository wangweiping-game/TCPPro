using API;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WaitPanel : IPanel
{
    [UIField(0)]
    public Text text_num;
    [UIField(1)]
    public Button btn_startFight;

    private Timer heartTimer;
    public override void Start()
    {
        base.Start();
        btn_startFight.onClick.AddListener(onClickStartFight);
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.NotifyRoomInfo, UpdateRoomInfo);
        heartTimer = TimerManager.GetInstance().createTimer(0.1f, heartEvent);
    }

    public override void Show(params object[] param)
    {
        base.Show(param);
        bool isRoomMaster = false;
        if (param.Length > 0)
            isRoomMaster = (bool)param[0];
        btn_startFight.gameObject.SetActive(isRoomMaster);
        if (isRoomMaster)
            heartTimer.start();
    }

    void onClickStartFight()
    {
        InitService.GetInstance().ReqStartFight();
    }

    void heartEvent()
    {
        Game.GetInstance().StartCoroutine(Game.GetInstance().createRoom("create", true));
    }


    void UpdateRoomInfo(MemoryStream ms)
    {
        MessageNotifyRoomInfo info = MessageNotifyRoomInfo.Parser.ParseFrom(ms);
        text_num.text = info.PlayerInfoArray.Count.ToString();

        if (info.FightState == 1)
        {
            Singleton<GameModel>.GetInstance().UID = info.SelfUid;
            Singleton<GameModel>.GetInstance().roomInfoList.Clear();
            Singleton<GameModel>.GetInstance().roomInfoList.AddRange(info.PlayerInfoArray);
            heartTimer.stop();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
        }
    }

    public override void OnDestory()
    {
        if (heartTimer != null)
            heartTimer.onDispose();
        NetworkManager.GetInstance().RemoveHandle((int)MSG_CS.NotifyRoomInfo);
        base.OnDestory();
    }
}
