using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : BaseCommand
{
    public override void excute(params object[] param)
    {
        if (param.Length > 0)
        {
            Debug.Log("Move Command " + param[0].ToString());
            Singleton<GameModel>.GetInstance().dispatchMoveCommand(param[0].ToString());
        }

        else
            Debug.Log("Move Command!");
    }
}
