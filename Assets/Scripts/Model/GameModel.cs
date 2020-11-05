using API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : EventDispatcher
{
    public static bool serverStartFlag = false;
    private string token = "";
    private string uid = "";
    public List<RoomPlayerInfo> roomInfoList = new List<RoomPlayerInfo>();
    public static string roomServerIp = "";
    private static string remoteServerUrl = "http://172.16.39.17:8080/demo/";
    public static string serverUrl
    {
        get { return remoteServerUrl; }
    }

    public string Token
    {
        get { return token; }
        set { token = value; }
    }

    public string UID
    {
        get { return uid; }
        set { uid = value; }
    }

    public void dispatchMoveCommand( string param)
    {
        EventObject e = new EventObject();
        e.obj = param;
        dispatchEvent("command_event", e);
    }

}
