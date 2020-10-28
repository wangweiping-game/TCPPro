using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : EventDispatcher
{

    private string token = "";

    public string Token
    {
        get { return token; }
        set { token = value; }
    }

}
