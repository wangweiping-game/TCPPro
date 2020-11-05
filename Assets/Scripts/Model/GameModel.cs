using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : EventDispatcher
{
    public static bool serverStartFlag = false;
    private string token = "";
    private string playerId = "";

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

    public string PlayerID
    {
        get { return playerId; }
        set { playerId = value; }
    }

}
