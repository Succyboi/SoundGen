using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoundVisualizer : MonoBehaviour
{
    public RawImage display;
    public Color displayColor;
    public float displaySize = 1;

    private int width;
    private int height;
    private List<float> buffer = new List<float>();
    private int lastFrequency;

    private void Start()
    {
        display.color = Color.white;

        width = Mathf.RoundToInt(display.rectTransform.rect.width);
        height = Mathf.RoundToInt(display.rectTransform.rect.height);
    }

    private void Update()
    {
        //get from buffer and display
        float[] visualData = new float[Mathf.RoundToInt(width * displaySize)];
        for(int d = 0; d < Mathf.Clamp(visualData.Length, 0, buffer.Count); d++)
        {
            visualData[d] = buffer[d];
        }

        display.texture = AudioTools.PaintWaveformSpectrum(visualData, width, height, displayColor, display.texture != null ? (Texture2D)display.texture : null);

        //remove from start of buffer
        buffer.RemoveRange(0, Mathf.Clamp(Mathf.RoundToInt(lastFrequency * Time.deltaTime), 0, buffer.Count));
    }

    public void AddToBuffer(AudioClip clip, bool clear = true)
    {
        //clear buffer
        if (clear)
        {
            buffer.Clear();
        }

        //add
        float[] data = new float[clip.samples];
        clip.GetData(data, 0);
        buffer.AddRange(new float[Mathf.RoundToInt(width * displaySize)]); //add blank up front
        buffer.AddRange(data);
        lastFrequency = clip.frequency;
    }
}
