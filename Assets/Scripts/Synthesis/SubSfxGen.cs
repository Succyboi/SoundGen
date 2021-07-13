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
    [Range(1, 8)] public int startCrush = 1;
    [Range(1, 8)] public int endCrush = 1;

    [HideInInspector] public AudioSource source;
    private LTROController input;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        input = FindObjectOfType<LTROController>();
    }

    private void Update()
    {
        //start to play sound
        if (input.startDown)
        {
            PlaySfx();
        }
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

        //quantize pitches
        float startPitch = Pitch.QuantizeFrequency(this.startPitch);
        float endPitch = Pitch.QuantizeFrequency(this.endPitch);

        //do audio
        for (int d = 0; d < data.Length; d++)
        {
            //run osc
            float pitch = Mathf.Lerp(startPitch, endPitch, (float)d / data.Length) + Mathf.Lerp(-pitchLfoAmount / 2, pitchLfoAmount / 2, pitchLfo.Run(rate));
            pitch = quantizePitch ? Pitch.QuantizeFrequency(pitch) : pitch;

            data[d] = osc.Run(
                pitch, 
                rate) * volumeEnv.Run(rate);

            //filter
            if (useLpf)
            {
                lpf.cutoff = Mathf.Clamp(filterEnv.Run(rate), 0.01f, 1f);
                data[d] = lpf.Run(data[d]);
            }
        }

        //do bitcrush
        for (int d = 0; d < data.Length - Mathf.RoundToInt(Mathf.Lerp(startCrush, endCrush, (float)d / data.Length)); d += Mathf.RoundToInt(Mathf.Lerp(startCrush, endCrush, (float)d / data.Length)))
        {
            //bitcrush
            float stepValue = 0f;
            int stepSize = Mathf.RoundToInt(Mathf.Lerp(startCrush, endCrush, (float)d / data.Length));

            //go through all data inbetween steps
            for (int i = 0; i < stepSize; i++)
            {
                stepValue += data[d + i];
            }

            //calculate the average
            stepValue /= stepSize;

            //and assign the average again
            for (int i = 0; i < stepSize; i++)
            {
                data[d + i] = stepValue;
            }
        }

        //assign data to clip
        AudioClip generatedClip = AudioClip.Create(System.Guid.NewGuid().ToString(), data.Length, 1, rate, false);
        generatedClip.SetData(data, 0);

        return generatedClip;
    }

    #region Randomisation

    public void RandomSound()
    {
        switch(Random.Range(0, 3))
        {
            case 0:
                Glitch();
                break;

            case 1:
                Explosion();
                break;

            case 2:
                Jump();
                break;

            case 3:
                break;
        }
    }

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

        //randomize bitcrush
        float crush = Random.value;
        startCrush = crush > 0.5 ? 1 : Random.Range(1, 8);
        endCrush = crush > 0.5 ? 1 : Random.Range(1, 8);

        //play
        PlaySfx();
    }

    public void Explosion()
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

        //randomize bitcrush
        float crush = Random.value;
        startCrush = 1;
        endCrush = crush > 0.5 ? 1 : 8;

        //play
        PlaySfx();
    }

    public void Jump()
    {
        //quantise pitch
        quantizePitch = false;

        //randomize start and end pitch
        startPitch = Pitch.NotesInHertz[Random.Range(Pitch.NotesInHertz.Length / 2, Pitch.NotesInHertz.Length)];
        endPitch = Pitch.NotesInHertz[Random.Range(0, Pitch.NotesInHertz.Length / 2)];

        //randomize pitch lfo
        pitchLfoAmount = 0;
        pitchLfoSpeedHz = 0;

        //randomize volume envelope 
        volumeEnv.attack = 0.01f;
        volumeEnv.sustain = 0.01f;
        volumeEnv.decay = Random.Range(0.25f, 0.75f);

        //randomize osc morph
        osc.morph = Random.Range(0, 0.75f);

        //randomize filter env and use
        useLpf = false;

        //randomize bitcrush
        startCrush = 1;
        endCrush = 1;

        //play
        PlaySfx();
    }

    #endregion
}