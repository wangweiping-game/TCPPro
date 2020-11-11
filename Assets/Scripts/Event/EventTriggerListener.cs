using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class EventTriggerListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    static public EventTriggerListener Get(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("BtnEventTriggerListener_go_is_NULL");
            return null;
        }
        else
        {
            EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
            if (listener == null)
                listener = go.AddComponent<EventTriggerListener>();
            return listener;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(gameObject);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null)
            onDown(gameObject);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null)
            onEnter(gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null)
            onExit(gameObject);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null)
            onUp(gameObject);
    }
    public void clearAllListener()
    {
        onClick = null;
        onDown = null;
        onExit = null;
        onUp = null;
        Destroy(gameObject.GetComponent<EventTriggerListener>());
    }
}
