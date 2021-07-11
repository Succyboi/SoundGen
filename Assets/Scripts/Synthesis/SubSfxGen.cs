using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubSfxGen : MonoBehaviour
{
    [Header("General")]
    public int rate = 22050;

    [Header("Synthesis")]
    public float frequency = 1000;
    public Envelope env;
    public BitCrusher bitCsrusher;
    public MorphOsc osc;

    public AudioClip GenerateSfx()
    {
        //reset stuff
        env.Trigger();

        //create data array
        float[] data = new float[Mathf.RoundToInt(env.duration * rate)];

        for(int d = 0; d < data.Length; d++)
        {
            data[d] = osc.Run(frequency, rate);

            //bitcrush
            //data[d] = bitCsrusher.Run(data[d]);
        }

        //assign data to clip
        AudioClip generatedClip = AudioClip.Create(name, data.Length, 1, rate, false);
        generatedClip.SetData(data, 0);

        return generatedClip;
    }
}