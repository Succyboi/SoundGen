using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SubSfxGen : MonoBehaviour
{
    [Header("General")]
    public int rate = 22050;

    [Header("Synthesis")]
    public bool quantizePitch;
    public float startPitch = 1000;
    public float endPitch = 1000;
    public Lfo pitchLfo;
    public float pitchLfoAmount;
    public float pitchLfoSpeedHz;
    public ASDEnvelope volumeEnv;
    public MorphOsc osc;
    public bool useLpf;
    public Lowpass lpf;
    public ASDEnvelope filterEnv;

    [HideInInspector] public AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlaySfx()
    {
        if(source.clip != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Destroy(source.clip);
            }
            else
            {
                DestroyImmediate(source.clip);
            }
#else
            Destroy(source.clip);
#endif
        }

        source.clip = GenerateSfx();
        source.Play();
    }

    public AudioClip GenerateSfx()
    {
        //reset stuff
        volumeEnv.Trigger();
        filterEnv.Trigger();
        osc.Reset();
        pitchLfo.Reset();
        pitchLfo.duration = 1 / pitchLfoSpeedHz;

        //create data array
        float[] data = new float[Mathf.RoundToInt(volumeEnv.duration * rate)];

        //do audio
        for(int d = 0; d < data.Length; d++)
        {
            float pitch = Mathf.Lerp(startPitch, endPitch, (float)d / data.Length) + Mathf.Lerp(-pitchLfoAmount / 2, pitchLfoAmount / 2, pitchLfo.Run(rate));
            pitch = quantizePitch ? Pitch.QuantizeFrequency(pitch) : pitch;

            data[d] = osc.Run(
                pitch, 
                rate) * volumeEnv.Run(rate);

            if (useLpf)
            {
                lpf.cutoff = Mathf.Clamp(filterEnv.Run(rate), 0.01f, 1f);
                data[d] = lpf.Run(data[d]);
            }
        }

        //assign data to clip
        AudioClip generatedClip = AudioClip.Create(System.Guid.NewGuid().ToString(), data.Length, 1, rate, false);
        generatedClip.SetData(data, 0);

        return generatedClip;
    }

    #region Randomisation

    public void Glitch()
    {
        //quantise pitch
        quantizePitch = Random.value > 0.5f;

        //randomize start and end pitch
        startPitch = Pitch.NotesInHertz[Random.Range(0, Pitch.NotesInHertz.Length)];
        endPitch = Pitch.NotesInHertz[Random.Range(0, Pitch.NotesInHertz.Length)];

        //randomize pitch lfo
        pitchLfoAmount = Random.value > 0.5f ? 0 : Random.Range(100f, 500f);
        pitchLfoSpeedHz = Random.Range(1f, 20f);

        //randomize volume envelope 
        volumeEnv.attack = Random.Range(0.01f, 1f);
        volumeEnv.decay = Random.Range(0.01f, 1f);
        volumeEnv.sustain = Random.Range(0.01f, 1f);

        //randomize osc morph
        osc.morph = Random.value;

        //randomize filter env and use
        useLpf = Random.value > 0.5f;

        filterEnv.attack = Random.Range(0.01f, 1f);
        filterEnv.decay = Random.Range(0.01f, 1f);
        filterEnv.sustain = Random.Range(0.01f, 1f);

        //play
        PlaySfx();
    }

    public void GlitchPluck()
    {
        //quantise pitch
        quantizePitch = Random.value > 0.5f;

        //randomize start and end pitch
        startPitch = Pitch.NotesInHertz[Random.Range(0, Pitch.NotesInHertz.Length)];
        endPitch = Pitch.NotesInHertz[Random.Range(0, Pitch.NotesInHertz.Length)];

        //randomize pitch lfo
        pitchLfoAmount = Random.value > 0.5f ? 0 : Random.Range(100f, 500f);
        pitchLfoSpeedHz = Random.Range(1f, 20f);

        //randomize volume envelope 
        volumeEnv.attack = 0.01f;
        volumeEnv.sustain = Random.Range(0.01f, 1f);
        volumeEnv.decay = Random.Range(0.01f, 1f);

        //randomize osc morph
        osc.morph = Random.value;

        //randomize filter env and use
        useLpf = Random.value > 0.5f;

        filterEnv.attack = 0.01f;
        filterEnv.sustain = Random.Range(0.01f, 1f);
        filterEnv.decay = Random.Range(0.01f, 1f);

        //play
        PlaySfx();
    }

    public void RandomExplosion()
    {
        //randomize volume envelope 
        volumeEnv.attack = 0.01f;
        volumeEnv.sustain = Random.value > 0.5f ? Random.Range(0.01f, 0.1f) : Random.Range(0.1f, 0.5f);
        volumeEnv.decay = volumeEnv.sustain < 0.1f ? Random.Range(0.01f, 1f) : Random.Range(1f, 2f);

        //randomize osc morph
        osc.morph = 1;

        //randomize filter env and use
        useLpf = true;

        filterEnv.attack = 0.01f;
        filterEnv.sustain = volumeEnv.sustain * Random.Range(0.25f, 0.75f);
        filterEnv.decay = volumeEnv.decay * Random.Range(0.25f, 0.75f);

        //play
        PlaySfx();
    }

    #endregion
}