using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenShot : MonoBehaviour
{
    [Range(1, 16)]
    public int superSize = 1;

    private void Update()
    {
        //only in editor, to assets/screenshots
        if (Application.isEditor && Keyboard.current[Key.F12].wasPressedThisFrame)
        {
            int availableIndex = 0;
            while (System.IO.File.Exists(Application.dataPath + "/Screenshots/" + availableIndex.ToString() + ".png"))
            {
                availableIndex++;
            }

            ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshots/" + availableIndex.ToString() + ".png", superSize);
        }
    }
}
