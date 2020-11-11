using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPanel 
{
    public ID_PANEL id;
    GameObject gameObject;
    public virtual void Start()
    {
        InitView();
        InitEvent();
    }

    public virtual void Update(){}

    public virtual void FixedUpdate(){}

    public virtual void Show(params object[] param)
    {
        if (gameObject != null)
            gameObject.SetActive(true);
    }

    public virtual void InitView() { }
    public virtual void InitEvent(){ }


    public GameObject Go
    {
        get
        {
            return gameObject;
        }
        set
        {
            if (gameObject == null)
                Start();
            gameObject = value;
        }
    }

    public bool IsLoad
    {
        get { return gameObject != null; }
    }

    public virtual void Hide()
    {
        if (gameObject == null)
            return;
        gameObject.SetActive(false);
    }
    public virtual void OnDestory() { }
    public void Destory()
    {
        if (gameObject == null)
            return;
        OnDestory();
        GameObject.Destroy(gameObject);
    }


}
