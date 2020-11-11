using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptPanel : IPanel
{
    [UIField(0)]
    public Animator ani;
    [UIField(1)]
    public Text text_prompt;

    private Timer timer;

    public override void Start()
    {
        base.Start();
        timer = new Timer(ani.GetCurrentAnimatorStateInfo(0).length);
        timer.setTimerHandle(EndHandle);
    }

    public override void Show(params object[] param)
    {
        base.Show(param);
        if (param.Length > 0)
            text_prompt.text = param[0].ToString();
        ani.Play(0);
        if (timer != null)
        {
            timer.stop();
            timer.start();
        }
            
    }

    void EndHandle()
    {
        if(timer != null)
        {
            timer.stop();
        }
        Hide();
    }
}
