using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFieldAttribute : Attribute
{
    int[] mIndexs;

    public UIFieldAttribute(params int[] indexs)
    {
        mIndexs = indexs;
    }

    public int[] Index
    {
        get
        {
            return mIndexs;
        }
    }
}
