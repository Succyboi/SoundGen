using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SubSfxGen : MonoBehaviour
{
    [Header("General")]
    public int rate = 22050;

    [Header("Synthesis")]
    public BitCrusher crusher;
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

    [Header("Saving")]
    public string preSaveName = "SFXO1_";

    [Header("Misc")]
    public SoundVisualizer soundVisualizer;

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

        //generate
        float[] sfxData = GenerateSfxData();
        source.clip = GenerateSfx(sfxData);

        //visualize
        if(soundVisualizer != null)
        {
            soundVisualizer.AddToBuffer(sfxData, rate);
        }

        //play
        source.Play();

        //update UI
        UpdateUI();
    }

    #region Generation

    public float[] GenerateSfxData()
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

            //crush bit depth
            data[d] = crusher.Run(data[d]);
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
        return data;
    }


    public AudioClip GenerateSfx(float[] SfxData)
    {
        //assign data to clip
        AudioClip generatedClip = AudioClip.Create(System.Guid.NewGuid().ToString(), SfxData.Length, 1, rate, false);
        generatedClip.SetData(SfxData, 0);

        return generatedClip;
    }

    #endregion

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
        Wave = Random.Range(0, 4);

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
        Wave = 3;

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
        Wave = Random.Range(0, 3);

        //randomize filter env and use
        useLpf = false;

        //randomize bitcrush
        startCrush = 1;
        endCrush = 1;

        //play
        PlaySfx();
    }

    #endregion

    #region UI Bindings

    //update all the values in the ui to make sense with what's happening in the component
    public void UpdateUI()
    {
        WaveDropdown.value = Wave;

        QuantizePitchToggle.isOn = QuantizePitch;

        StartPitchSlider.value = StartPitch;
        EndPitchSlider.value = EndPitch;

        PitchLfoAmountSlider.value = PitchLfoAmount;
        PitchLfoSpeedHzSlider.value = PitchLfoSpeedHz;

        VolumeAttackSlider.value = VolumeAttack;
        VolumeSustainSlider.value = VolumeSustain;
        VolumeDecaySlider.value = VolumeDecay;

        UseFilterToggle.isOn = UseFilter;

        FilterAttackSlider.value = FilterAttack;
        FilterSustainSlider.value = FilterSustain;
        FilterDecaySlider.value = FilterDecay;

        StartCrushSlider.value = StartCrush;
        EndCrushSlider.value = EndCrush;
    }

    [Header("UI Bindings")]
    public Dropdown WaveDropdown;
    public int Wave
    {
        set
        {
            osc.morph = Mathf.Clamp01(value / 4f);
        }
        get
        {
            return Mathf.RoundToInt(osc.morph * 4);
        }
    }

    public Toggle QuantizePitchToggle;
    public bool QuantizePitch
    {
        get
        {
            return quantizePitch;
        }
        set
        {
            quantizePitch = value;
        }
    }

    public Slider StartPitchSlider;
    public float StartPitch
    {
        get
        {
            return startPitch;
        }
        set
        {
            startPitch = value;
        }
    }

    public Slider EndPitchSlider;
    public float EndPitch
    {
        get
        {
            return endPitch;
        }
        set
        {
            endPitch = value;
        }
    }

    public Slider PitchLfoAmountSlider;
    public float PitchLfoAmount
    {
        get
        {
            return pitchLfoAmount;
        }
        set
        {
            pitchLfoAmount = value;
        }
    }

    public Slider PitchLfoSpeedHzSlider;
    public float PitchLfoSpeedHz
    {
        get
        {
            return pitchLfoSpeedHz;
        }
        set
        {
            pitchLfoSpeedHz = value;
        }
    }

    public Slider VolumeAttackSlider;
    public float VolumeAttack
    {
        get
        {
            return volumeEnv.attack;
        }
        set
        {
            volumeEnv.attack = value;
        }
    }

    public Slider VolumeSustainSlider;
    public float VolumeSustain
    {
        get
        {
            return volumeEnv.sustain;
        }
        set
        {
            volumeEnv.sustain = value;
        }
    }

    public Slider VolumeDecaySlider;
    public float VolumeDecay
    {
        get
        {
            return volumeEnv.decay;
        }
        set
        {
            volumeEnv.decay = value;
        }
    }

    public Toggle UseFilterToggle;
    public bool UseFilter
    {
        get
        {
            return useLpf;
        }
        set
        {
            useLpf = value;
        }
    }

    public Slider FilterAttackSlider;
    public float FilterAttack
    {
        get
        {
            return filterEnv.attack;
        }
        set
        {
            filterEnv.attack = value;
        }
    }

    public Slider FilterSustainSlider;
    public float FilterSustain
    {
        get
        {
            return filterEnv.sustain;
        }
        set
        {
            filterEnv.sustain = value;
        }
    }

    public Slider FilterDecaySlider;
    public float FilterDecay
    {
        get
        {
            return filterEnv.decay;
        }
        set
        {
            filterEnv.decay = value;
        }
    }

    public Slider StartCrushSlider;
    public float StartCrush
    {
        get
        {
            return startCrush;
        }
        set
        {
            startCrush = Mathf.RoundToInt(value);
        }
    }

    public Slider EndCrushSlider;
    public float EndCrush
    {
        get
        {
            return endCrush;
        }
        set
        {
            endCrush = Mathf.RoundToInt(value);
        }
    }

    #endregion

    #region Saving

    public bool Save()
    {
        if(source.clip != null)
        {
            string downloadPath = KnownFolders.GetPath(KnownFolder.Downloads);

            //find available file name
            int availableIndex = 0;
            while (System.IO.File.Exists(downloadPath + "/" + preSaveName + availableIndex.ToString() + ".wav"))
            {
                availableIndex++;
            }

            System.IO.File.WriteAllBytes(downloadPath + "/" + preSaveName + availableIndex.ToString() + ".wav", SavWav.FloatsToWavData(GenerateSfxData(), rate));

            return true;
        }
        else
        {
            return false;
        }
    }

    //browser only
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);
    [HideInInspector] public bool downloadSuccesful = false;

    public void Download()
    {
        //"download" file
        byte[] soundData = SavWav.FloatsToWavData(GenerateSfxData(), rate);
        DownloadFile(gameObject.name, "OnFileDownload", preSaveName.Remove(preSaveName.Length - 1) + ".wav", soundData, soundData.Length);
    }

    public void OnDownload()
    {
        downloadSuccesful = true;
    }

    #endregion
}