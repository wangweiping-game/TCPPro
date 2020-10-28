using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EventDispatcher
{
	private Dictionary<string, Action<EventObject>> eventHandleDic = new Dictionary<string, Action<EventObject>>();
	public void addEvent(string type,Action<EventObject> handle)
	{
        Action<EventObject> ac;
        if (eventHandleDic.TryGetValue(type, out ac))
        {            
            if(null != ac)
            {
                Delegate[] list = ac.GetInvocationList();
                int len = list.Length;
                for (int i = 0; i < len; ++i)
                {
                    Action<EventObject> acTp = list[i] as Action<EventObject>;
                    if (acTp == handle)
                    {
                        Debug.LogWarning("重复增加事件处理方法:" + handle.ToString());
                        return;
                    }
                }
            }
            eventHandleDic[type] += handle;
		} else {
            eventHandleDic[type] = handle;
		}
	}

    public void removeEvent(string type, Action<EventObject> handle)
    {
        if (eventHandleDic.ContainsKey(type))
        {
            eventHandleDic[type] -= handle;
        }
    }
    
        public void dispatchEvent(string type,EventObject e = null )
        {
            if (e == null) e = new EventObject();
            if (eventHandleDic.ContainsKey (type)) {
                Action<EventObject> ac = eventHandleDic[type];
                if (null != ac)
                {
                    if(null != e) e.setSender(this);
                    ac(e);
                }
            }
        }
    
    public void dispatchEvent(string type, object obj)
    {
        EventObject e = new EventObject();
        e.obj = obj;
        if (eventHandleDic.ContainsKey(type))
        {
            Action<EventObject> ac = eventHandleDic[type];
            if (null != ac)
            {
                if (null != e) e.setSender(this);
                ac(e);
            }
        }
    }

    public virtual void onClear()
    {
        eventHandleDic.Clear();
    }

}