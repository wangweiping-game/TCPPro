﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        NetStateManager.GetInstance().Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        NetStateManager.GetInstance().Update();
        TimerManager.GetInstance().Update(Time.deltaTime);
    }
    void OnApplicationQuit()
    {
        NetworkManager.GetInstance().closeConnect();
    }
}