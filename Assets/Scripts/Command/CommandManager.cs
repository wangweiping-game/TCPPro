using API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager :Singleton<CommandManager>
{
    Dictionary<Command, BaseCommand> commandsDic = new Dictionary<Command, BaseCommand>();

   public CommandManager()
    {
        commandsDic[Command.Move] = new MoveCommand();
    }

    public void doCommand(List<Operation> ope)
    {
        for (int i = 0; i < ope.Count; i++)
        {
            Debug.Log("command :" + ope[i].CommandType.ToString());
            if(commandsDic.ContainsKey(ope[i].CommandType))
            {
                commandsDic[ope[i].CommandType].excute(ope[i].Data);
            }
        }
    }
}
