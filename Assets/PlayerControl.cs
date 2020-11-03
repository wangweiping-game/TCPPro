using API;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Dictionary<string, GameObject> playerDic = new Dictionary<string, GameObject>();
    Operation opetion = new Operation();

    void Start()
    {
        playerDic.Add(Singleton<GameModel>.GetInstance().PlayerID, gameObject);
        gameObject.name = Singleton<GameModel>.GetInstance().PlayerID;
        NetworkManager.GetInstance().AddHandle((int)MSG_CS.NotifySyncOperations, NotifySynOperations);
        resetOffset();
    }

    void Update()
    {
        float MoveH = Input.GetAxis("Horizontal");
        float MoveV = Input.GetAxis("Vertical");
        opetion.XOffset += MoveH * Time.deltaTime * 100;
        opetion.ZOffset += MoveV * Time.deltaTime * 100;
        sendOffsetToServer();
    }

    void sendOffsetToServer()
    {
        if (opetion.XOffset != 0 || opetion.ZOffset != 0)
        {
            InitService.GetInstance().NotifyClientOperations(opetion.XOffset, opetion.ZOffset);
            resetOffset();
        }
    }
    void resetOffset()
    {
        opetion.XOffset = 0;
        opetion.ZOffset = 0;
    }
    public void NotifySynOperations(MemoryStream stream)
    {
        MessageNotifySyncOperations res = MessageNotifySyncOperations.Parser.ParseFrom(stream);
        foreach (var item in res.PlayerOperations)
        {
            if(!playerDic.ContainsKey(item.PlayerId))
            {
                GameObject obj = Resources.Load("Sphere") as GameObject;
                obj = GameObject.Instantiate(obj);
                obj.transform.parent = gameObject.transform.parent;
                obj.name = item.PlayerId;
                obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                obj.transform.localPosition = new Vector3(0, 0.05f, 0);
                playerDic.Add(item.PlayerId, obj);
            }
            playerDic[item.PlayerId].transform.Translate(new Vector3(item.XOffset, 0, item.ZOffset));
            Debug.Log("同步 " + item.PlayerId + " 位置：" + item.XOffset + "  " + item.ZOffset);
        }

    }
}
