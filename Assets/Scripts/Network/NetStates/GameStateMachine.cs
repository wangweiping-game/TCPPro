using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateMachine
{
	private GameState currentState;
	private Dictionary<string, GameState> stateList = new Dictionary<string, GameState>();

	public GameStateMachine()
	{
		
	}

	public void addState(GameState state)
	{
		string name = state.getName();
		if (stateList.ContainsKey (name))
        {
			return;
		}
		stateList.Add(name,state);
	}

	public void changeState(string stateName)
	{
		if (null != currentState)
		{
			if(currentState.getName() == stateName)
				return;
			currentState.exit ();
			currentState = null;
		}
        GameState state;
		if (stateList.TryGetValue (stateName, out state))
        {
			currentState = state;
			currentState.enter ();
		}
        else
        {
			Debug.Log("没有状态：" + stateName);
		}
	}

	public void update()
	{
		if (null != currentState)
			currentState.update ();
	}

}
