using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : EventDispatcher
{

    private string token = "";
    private string playerId = "";

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
