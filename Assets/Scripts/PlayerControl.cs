using API;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Dictionary<string, GameObject> playerDic = new Dictionary<string, GameObject>();
    float xOffset = 10, zOffset = 20;

    Operation opetion = new Operation();

    private void Awake()
    {
        Singleton<GameModel>.GetInstance().addEvent("command_event", MoveCallback);
    }
    void Start()
    {
        List<RoomPlayerInfo> roominfo = Singleton<GameModel>.GetInstance().roomInfoList;

        foreach (var item in roominfo)
        {
            if (!playerDic.ContainsKey(item.Uid))
            {
                GameObject obj = Resources.Load("Sphere") as GameObject;
                obj = GameObject.Instantiate(obj);
                obj.transform.parent = gameObject.transform;
                obj.name = item.Uid;
                obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                obj.transform.localPosition = new Vector3(item.XInitialPos, 0.05f, item.ZInitialPos);
                playerDic.Add(item.Uid, obj);
            }
           // playerDic[item.Uid].transform.Translate(new Vector3(item.XInitialPos, 0, item.ZInitialPos));
            //Debug.Log("同步 " + item.Uid + " 位置：" + item.XInitialPos + "  " + item.ZInitialPos);
        }
    }

    void Update()
    {
        //float MoveH = Input.GetAxis("Horizontal");
        //float MoveV = Input.GetAxis("Vertical");
        //xOffset += MoveH * Time.deltaTime * 100;
        //zOffset += MoveV * Time.deltaTime * 100;
        //sendOffsetToServer();
    }

    public void sendOffsetToServer()
    {
        if (xOffset != 0 || zOffset != 0)
        {
            InitService.GetInstance().NotifyClientOperations(Command.Move, xOffset+"_"+zOffset);
            resetOffset();
        }
    }
    void resetOffset()
    {
        //xOffset = 0;
        //zOffset = 0;
        xOffset++;
        zOffset++;
    }
    //public void NotifySynOperations(MemoryStream stream)
    //{
        //MessageNotifySyncOperations res = MessageNotifySyncOperations.Parser.ParseFrom(stream);
        //foreach (var item in res.PlayerOperations)
        //{
        //    if(!playerDic.ContainsKey(item.PlayerId))
        //    {
        //        GameObject obj = Resources.Load("Sphere") as GameObject;
        //        obj = GameObject.Instantiate(obj);
        //        obj.transform.parent = gameObject.transform.parent;
        //        obj.name = item.PlayerId;
        //        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //        obj.transform.localPosition = new Vector3(0, 0.05f, 0);
        //        playerDic.Add(item.PlayerId, obj);
        //    }
        //    playerDic[item.PlayerId].transform.Translate(new Vector3(item.XOffset, 0, item.ZOffset));
        //    Debug.Log("同步 " + item.PlayerId + " 位置：" + item.XOffset + "  " + item.ZOffset);
        //}

    //}

    public void onClickDirectionBtn(string direction)
    {
        InitService.GetInstance().NotifyClientOperations(Command.Move, direction);
    }

    public void MoveCallback(EventObject eo)
    {
        string str = eo.obj.ToString();
        string[] datas = str.Split('_');
        if (datas.Length <= 0)
            return;
        if (playerDic.ContainsKey(datas[0]))
        {
            playerDic[datas[0]].transform.localPosition = 
                new Vector3(playerDic[datas[0]].transform.localPosition.x + int.Parse(datas[1])*0.1f,0.05f,
                playerDic[datas[0]].transform.localPosition.z + int.Parse(datas[2]) * 0.1f);
        }
    }

    private void OnDestroy()
    {
        Singleton<GameModel>.GetInstance().removeEvent("command_event", MoveCallback);
    }
}
