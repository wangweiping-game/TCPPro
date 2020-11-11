
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginPanel : IPanel
{
    [UIField(0)]
    public Button Btn_CreateRoom;
    [UIField(1)]
    public Button Btn_JoinRoom;

    public override void Start()
    {
        base.Start();
        Btn_CreateRoom.onClick.AddListener(onClickCreateRoomBtn);
        Btn_JoinRoom.onClick.AddListener(onClickJoinRoomBtn);
    }
    public override void Show(params object[] param)
    {

    }

    void onClickCreateRoomBtn()
    {
        Debug.Log("onClickCreateRoomBtn");
        Game.GetInstance().StartCoroutine(Game.GetInstance().createRoom("create"));
    }

    void onClickJoinRoomBtn()
    {
        Debug.Log("onClickJoinRoomBtn");
        Game.GetInstance().StartCoroutine(Game.GetInstance().createRoom("get"));
    }

}
