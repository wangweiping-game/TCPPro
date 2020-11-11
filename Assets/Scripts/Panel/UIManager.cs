using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ID_PANEL 
{
    ID_NONE = 0,
    LoginPanel ,
    PromptPanel ,
    WaitPanel,
}

public class UIManager : Singleton<UIManager>
{
    Dictionary<ID_PANEL, IPanel> mPanels = new Dictionary<ID_PANEL, IPanel>();
    IPanel curShowPanel = null;
    private GameObject UIPanel = null;

    public void Start()
    {
        UIPanel = GameObject.Find("Canvas/UIPanel");
    }
    public void ShowPanel(ID_PANEL panelId,params object[] args)
    {
        IPanel panel;
        if (!mPanels.ContainsKey(panelId))
        {
            System.Type type = System.Type.GetType(panelId.ToString());
            System.Object objcet = System.Activator.CreateInstance(type);
            panel = (IPanel)objcet;
            mPanels[panelId] = panel;
        }
        else
        {
            panel = mPanels[panelId];
        }
        if(!panel.IsLoad)
        {
            GameObject go = CreateUIPrefab(panelId.ToString());
            UISerializeManager.GetInstance().DeSerialize(panel, go);
            panel.Go = go;
        }
        panel.Show(args);
        if (panelId != ID_PANEL.PromptPanel)
        {
            if (curShowPanel != null)
            {
                curShowPanel.Hide();
            }
            curShowPanel = panel;
        }
        else
        {
            panel.Go.transform.SetAsLastSibling();
        }
    }

    GameObject CreateUIPrefab(string name)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(name)) as GameObject;
        go.name = name;
        go.transform.SetParent(UIPanel.transform);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.sizeDelta = Vector2.zero;
        rect.localPosition = Vector3.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        return go;
    }

}
