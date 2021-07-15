using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHelper : MonoBehaviour
{
    public Font UIFont;

    private void Start()
    {
        foreach(Text text in Resources.FindObjectsOfTypeAll<Text>())
        {
            text.font = UIFont;
        }
    }
}
