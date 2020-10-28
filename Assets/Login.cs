using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;

public class Login : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ClickStartServer()
    {
        Singleton<SocketServer>.GetInstance().Start();
    }
    public void ClickStartConnectServer()
    {
        NetStateManager.GetInstance().startConnect();
    }

    public void ClickLogin()
    {
        InitService.GetInstance().onLoginRequst();
    }
}
