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

    public void onClick()
    {
        NetworkManager.GetInstance().startConnect("192.168.1.102", 1234);
    }

    public void send()
    {
        NetworkManager.GetInstance().SendMessage(1, "hello world!");
    }
}
