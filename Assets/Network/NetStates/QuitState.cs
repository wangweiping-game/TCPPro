using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitState:BaseNetState
{
    public QuitState()
    {
        name = NET_STATE.STATE_QUIT;
    }

    public override void enter()
    {
        NetworkManager.GetInstance().onClear();
    }
}
