using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public int[] bitDepths;
    public int[] bitRates;
    public Vector2Int[] resolutions;
    public GameObject[] hiddenOnWebGL;
    public GameObject controls;
    public Selectable controlsButton;
    public GameObject settings;
    public Selectable settingsButton;

    private SubSfxGen sfx;

    private void Start()
    {
        sfx = FindObjectOfType<SubSfxGen>();

        Screen.SetResolution(resolutions[0].x, resolutions[0].y, false);

        if(Application.platform == RuntimePlatform.WebGLPlayer)
        {
            foreach (GameObject hide in hiddenOnWebGL)
            {
                hide.SetActive(false);
            }
        }
    }

    public void ToggleSettings()
    {
        if (!settings.activeInHierarchy) //to settings
        {
            settings.SetActive(true);
            controls.SetActive(false);

            settingsButton.Select();
        }
        else //from settings
        {
            settings.SetActive(false);
            controls.SetActive(true);

            controlsButton.Select();
        }
    }

    public void SwitchResolution(int resolution)
    {
        Screen.SetResolution(resolutions[resolution].x, resolutions[resolution].y, false);
    }

    public void SwitchBitDepth(int bitDepth)
    {
        sfx.crusher.bitDepth = bitDepths[bitDepth];
    }

    public void SwitchBitRate(int bitRate)
    {
        sfx.rate = bitRates[bitRate];
    }
}
