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

    void Start()
    {
        playerDic.Add(Singleton<GameModel>.GetInstance().PlayerID, gameObject);
        gameObject.name = Singleton<GameModel>.GetInstance().PlayerID;
        //NetworkManager.GetInstance().AddHandle((int)MSG_CS.NotifySyncOperations, NotifySynOperations);
        resetOffset();
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
    public void NotifySynOperations(MemoryStream stream)
    {
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

    }
}
