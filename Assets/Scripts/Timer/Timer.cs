using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Timer 
{
	Action timerHandle;
	private float m_Timer = 0;
	private bool m_StopFlag = true;
	private bool m_EndFlag = false;
	private float m_Delay;
    private int m_loop = 0;
    List<CallBackParam> CallBacks;

    public float Delay
    {
        set
        {
            m_Delay = value;
        }
        get
        {
            return m_Delay;
        }
    }
    public void RegisterEvent(TimerCallBack tcb, params System.Object[] args)
    {
        CallBacks.Add(new CallBackParam(tcb, args));
    }
    public Timer(float delay)
    {
        m_Delay = delay;
        CallBacks = new List<CallBackParam>();
        TimerManager.GetInstance().addTimer(this);
    }
    public Timer(float delay, int mLoop)
    {
        m_Delay = delay;
        m_loop = mLoop;
        CallBacks = new List<CallBackParam>();
        TimerManager.GetInstance().addTimer(this);
    }

	public void setTimerHandle(Action handle)
	{
		timerHandle = handle;
	}

	protected void update()
	{
		if(null != timerHandle)
            timerHandle();
        for (int i = 0; i < CallBacks.Count; i++)
        {
            CallBackParam cb = CallBacks[i];
            if (cb != null)
                cb.callback(cb.param);
        }
	}

	public void onTime(float timeElapsed)
	{
		if(isEnd())
            return;
		if(isStop())
            return;
		if(m_Delay > 0) 
		{
			m_Timer += timeElapsed;			
			while(m_Timer >= m_Delay)
			{
				update();
                if (m_loop == 1)
                {
                    onDispose();
//                    onDispose();
                    break;
                }
				m_Timer -= m_Delay;
                if (isEnd())
                {
                    break;
                }
                if (isStop())
                {
                    break;
                }
			}
		}
	}
	public void start()
	{
        if (Application.platform == RuntimePlatform.WindowsEditor)
            if (isEnd()) 
				Debug.LogWarning("已释放");
		m_StopFlag = false;
	}
	public void reStart()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            if (isEnd()) 
				Debug.LogWarning("已释放");
        m_StopFlag = false;
		m_Timer = 0;
	}
	public void stop()
	{
		m_StopFlag = true;
	}
	public bool isStop()
	{
		return m_StopFlag;
	}
	public bool isEnd()
	{
		return m_EndFlag;
	}
	public void onDispose()
	{
		stop();
		timerHandle = null;
		m_EndFlag = true;
	}
}
