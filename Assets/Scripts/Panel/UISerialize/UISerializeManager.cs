using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UISerializeManager : Singleton<UISerializeManager>
{
    public void DeSerialize(object o,GameObject go)
    {
        UIStore store = go.GetComponent<UIStore>();
        if (store == null)
            return;
        System.Type type = o.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;
        FieldInfo[] filds = type.GetFields(flags);
        foreach(var m in filds)
        {
            Attribute attr = Attribute.GetCustomAttribute(m, typeof(UIFieldAttribute));
            if (attr == null)
                continue;
            UIFieldAttribute realyAttr = attr as UIFieldAttribute;
            if(realyAttr.Index.Length > 1)
            {
                int realyCount = realyAttr.Index.Length;
                int storeCount = store.widgets.Length;
                string fieldName = m.FieldType.Name;
                fieldName = fieldName.Remove(fieldName.Length - 2);
                System.Type elemType = GetFieldType(fieldName);
                System.Array array = System.Array.CreateInstance(elemType, realyCount);
                for (int i = 0; i < realyCount; i++)
                {
                    int index = realyAttr.Index[i];
                    if (index >= storeCount)
                        continue;
                    GameObject gameObj = store.widgets[index];
                    if (elemType.IsSubclassOf(typeof(UnityEngine.Component)))
                    {
                        Component c = gameObj.GetComponent(elemType);
                        array.SetValue(c, i);
                    }
                    else if(elemType == typeof(UnityEngine.GameObject))
                    {
                        array.SetValue(gameObj, i);
                    }
                    else
                    {
                        System.Object obj = System.Activator.CreateInstance(elemType);
                        array.SetValue(obj, i);
                        DeSerialize(obj, gameObj);
                    }
                }
                m.SetValue(o, array);
            }
            else
            {
                int index = realyAttr.Index[0];
                if (index < store.widgets.Length)
                {
                    GameObject child = store.widgets[index];
                    if (child == null)
                        continue;
                    if (m.FieldType.IsSubclassOf(typeof(UnityEngine.Component)))
                    {
                        UnityEngine.Component c = child.GetComponent(m.FieldType);
                        m.SetValue(o, c);
                    }
                    else if (m.FieldType == typeof(UnityEngine.GameObject))
                    {
                        m.SetValue(o, child);
                    }
                    else
                    {
                        System.Object childObject = System.Activator.CreateInstance(m.FieldType);
                        m.SetValue(o, childObject);
                        DeSerialize(childObject, child);
                    }
                }
            }
        }
    }

    public System.Type GetFieldType(string typeName)
    {
        System.Type type = System.Type.GetType(typeName);
        if(type == null)
        {
            switch (typeName)
            {
                case "Button":
                    type = typeof(UnityEngine.UI.Button);
                    break;
                case "Text":
                    type = typeof(UnityEngine.UI.Text);
                    break;
                case "Image":
                    type = typeof(UnityEngine.UI.Image);
                    break;
                case "Toggle":
                    type = typeof(UnityEngine.UI.Toggle);
                    break;
                case "GameObject":
                    type = typeof(UnityEngine.GameObject);
                    break;
                case "Transform":
                    type = typeof(UnityEngine.Transform);
                    break;
                case "InputField":
                    type = typeof(UnityEngine.UI.InputField);
                    break;
                case "RawImage":
                    type = typeof(UnityEngine.UI.RawImage);
                    break;
                case "Slider":
                    type = typeof(UnityEngine.UI.Slider);
                    break;
                case "ScrollRect":
                    type = typeof(UnityEngine.UI.ScrollRect);
                    break;
                case "RectTransform":
                    type = typeof(UnityEngine.RectTransform);
                    break;
                case "ToggleGroup":
                    type = typeof(UnityEngine.UI.ToggleGroup);
                    break;
                case "CanvasGroup":
                    type = typeof(UnityEngine.CanvasGroup);
                    break;
                case "VerticalLayoutGroup":
                    type = typeof(UnityEngine.UI.VerticalLayoutGroup);
                    break;
                case "HorizontalLayoutGroup":
                    type = typeof(UnityEngine.UI.HorizontalLayoutGroup);
                    break;
                case "Animator":
                    type = typeof(UnityEngine.Animator);
                    break;
                case "LayoutElement":
                    type = typeof(UnityEngine.UI.LayoutElement);
                    break;
            }
        }
        return type;
    }
}
