using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TimerManager : Singleton<TimerManager>
{
    private List<Timer> timerList = new List<Timer>();
    public void addTimer(Timer timer)
    {
        if (timerList.Contains(timer))
            return;
        timerList.Add(timer);
    }

    //便于做对象池
    public Timer createTimer(float delay, Action handle)
    {
        Timer timer = new Timer(delay);
        timer.setTimerHandle(handle);
        addTimer(timer);
        return timer;
    }

    public void Update(float timeElapsed)
    {
        int len = timerList.Count;
        Timer timer;
        for (int i = len - 1; i >= 0; i--)
        {
            timer = timerList[i];
            if (!timer.isEnd())
            {
                timer.onTime(timeElapsed);
            }
            else
            {
                timerList.RemoveAt(i);
            }
        }
    }
    public void StopAllTimer()
    {
        int len = timerList.Count;
        for (int i = 0; i < len; i++)
        {
            timerList[i].onDispose();
        }
    }

}
public class CallBackParam
{
    public TimerCallBack callback;
    public System.Object[] param;
    public CallBackParam(TimerCallBack cb,params System.Object[] args)
    {
        callback = cb;
        param = args;
    }
}
public delegate void TimerCallBack(params System.Object[] param);
