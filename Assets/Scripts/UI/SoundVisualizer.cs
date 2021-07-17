using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoundVisualizer : MonoBehaviour
{
    [Header("Waveform")]
    public RawImage display;
    public Color displayColor;
    public float displaySize = 1;
    public int width = 240;
    public int height = 135;

    [Header("Particles")]
    public int averageRange = 100;
    public AnimationCurve sizeCurve;
    public Vector2 sizeRange;
    public float smoothAmount;
    public ParticleSystem particleSystem;

    private float avgVolumeVelocity;
    private float avgVolume;

    private List<float> buffer = new List<float>();
    private int lastFrequency;

    private void Start()
    {
        display.color = Color.white;
    }

    private void Update()
    {
        //WAVEFORM
        //get from buffer and display
        float[] visualData = new float[Mathf.RoundToInt(width * displaySize)];
        for(int d = 0; d < Mathf.Clamp(visualData.Length, 0, buffer.Count); d++)
        {
            visualData[d] = buffer[d];
        }

        display.texture = AudioTools.PaintWaveformSpectrum(visualData, width, height, displayColor, display.texture != null ? (Texture2D)display.texture : null);

        //remove from start of buffer
        buffer.RemoveRange(0, Mathf.Clamp(Mathf.RoundToInt(lastFrequency * Time.deltaTime), 0, buffer.Count));

        //PARTICLES
        avgVolume = Mathf.SmoothDamp(avgVolume, buffer.Count > 0 ?
            buffer.GetRange(0, Mathf.Clamp(averageRange, 0, buffer.Count)).Average()
            : 0, ref avgVolumeVelocity, smoothAmount);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.sizeMultiplier = Mathf.Lerp(sizeRange.x, sizeRange.y, avgVolume);
    }

    public void AddToBuffer(float[] data, int rate, bool clear = true)
    {
        //clear buffer
        if (clear)
        {
            buffer.Clear();
        }

        //add
        buffer.AddRange(new float[Mathf.RoundToInt(width * displaySize)]); //add blank up front
        buffer.AddRange(data);
        lastFrequency = rate;
    }
}
