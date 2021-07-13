using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundVisualizer : MonoBehaviour
{
    public RawImage display;
    public Color displayColor;
    public float displaySize = 1;

    private int width;
    private int height;
    private bool dataAvailable;
    private float[] data;
    private float[] newData = new float[0];

    private void Start()
    {
        width = Mathf.RoundToInt(display.rectTransform.rect.width);
        height = Mathf.RoundToInt(display.rectTransform.rect.height);

        newData = new float[Mathf.RoundToInt(width * displaySize)];
    }

    private void Update()
    {
        //get data if available
        if (dataAvailable)
        {
            data = newData;
            display.color = Color.white;
        }

        display.texture = AudioTools.PaintWaveformSpectrum(data, width, height, displayColor);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        //set unavailable
        dataAvailable = false;

        for (int d = 0; d < newData.Length; d++)
        {
            newData[newData.Length - 1 -  d] = data[Mathf.RoundToInt((float)d / newData.Length * (data.Length - 1))];
        }

        //set available
        dataAvailable = true;
    }
}
