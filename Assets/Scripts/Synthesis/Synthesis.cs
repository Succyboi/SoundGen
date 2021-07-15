using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

//weird gui stuff
public static class AudioTools
{
    public static Texture2D PaintWaveformSpectrum(float[] clipContents, int width, int height, Color col, Texture2D tex = null)
    {
        if (width < 1 || height < 1)
        {
            return null;
        }

        Color backgroundColor = new Color(0, 0, 0, 0);

        //only create if null
        if(tex == null)
        {
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        //create thing
        if (clipContents != null)
        {
            //condense clip contents
            float[] waveform = new float[width];
            for (int w = 0; w < waveform.Length; w++)
            {
                waveform[w] = clipContents[Mathf.RoundToInt((float)w / waveform.Length * (clipContents.Length - 1))];
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, backgroundColor);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < waveform[x] * ((float)height * .5f); y++)
                {
                    tex.SetPixel(x, (height / 2) + y, col);
                    tex.SetPixel(x, (height / 2) - y, col);
                }
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, col);
                }
            }
        }

        tex.Apply();

        return tex;
    }
}

[System.Serializable]
public class BitCrusher
{
    public int bitDepth = 256;

    public float Run(float sampleIn)
    {
        //apply bit depth
        return Mathf.Round(sampleIn * bitDepth) / (bitDepth);
    }
}

[System.Serializable]
public class MorphOsc
{
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(0f, 1f)]
    public float morph = 0;
    public AnimationCurve sineMorph;
    public AnimationCurve sawMorph;
    public AnimationCurve squareMorph;
    public AnimationCurve noiseMorph;

    private uint pos = 0;

    public float Run(int note, int rate)
    {
        float freq = Pitch.NoteToPitch(note);

        //get all waves
        float sine = RunSine(freq, rate);
        float saw = RunSaw(freq, rate);
        float square = RunSquare(freq, rate);
        float noise = Pitch.SimpleRand(pos);

        //increment to keep track of position
        pos++;

        return (sine * sineMorph.Evaluate(morph)
            + saw * sawMorph.Evaluate(morph)
            + square * squareMorph.Evaluate(morph)
            + noise * noiseMorph.Evaluate(morph)) * volume;
    }

    public float Run(float freq, int rate)
    {
        //get all waves
        float sine = RunSine(freq, rate);
        float saw = RunSaw(freq, rate);
        float square = RunSquare(freq, rate);
        float noise = Pitch.SimpleRand(pos);

        //increment to keep track of position
        pos++;

        return (sine * sineMorph.Evaluate(morph)
            + saw * sawMorph.Evaluate(morph)
            + square * squareMorph.Evaluate(morph)
            + noise * noiseMorph.Evaluate(morph)) * volume;
    }

    public float RunSine(float freq, int rate)
    {
        return Mathf.Sin((2 * Mathf.PI * pos * freq) / rate);
    }

    public float RunSaw(float freq, int rate)
    {
        return ((pos * (2 * freq / rate)) % 2) - 1;
    }

    public float RunSquare(float freq, int rate)
    {
        return ((pos * (2 * freq / rate)) % 2) - 1 >= 0 ? 1 : -1;
    }

    public void Reset()
    {
        pos = 0;
    }
}

[System.Serializable]
public class NoiseGen
{
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private uint pos = 0;

    public float Run()
    {
        pos++;

        return Pitch.SimpleRand(pos);
    }
}

[System.Serializable]
public class Envelope
{
    public AnimationCurve curve;
    public float duration;

    private uint pos = uint.MaxValue;

    public float Run(int rate)
    {
        float curvePos = pos / (duration * rate);

        if(curvePos < 1)
        {
            //increment to keep track of position
            pos++;

            return Mathf.Clamp01(curve.Evaluate(curvePos));
        }
        else
        {
            return Mathf.Clamp01(curve.Evaluate(1));
        }
    }

    public virtual void Trigger()
    {
        pos = 0;
    }
}

[System.Serializable]
public class ASDEnvelope : Envelope
{
    [Header("ASD")]
    public float attack;
    public float sustain;
    public float decay;

    public override void Trigger()
    {
        //set duration
        duration = attack + sustain + decay;

        //correct if curve is weird
        if(curve.keys.Length != 4)
        {
            //remove all
            for(int k = 0; k < curve.keys.Length; k++)
            {
                curve.RemoveKey(k);
            }

            //add keys
            for (int k = 0; k < 4; k++)
            {
                curve.AddKey(k, 0);
            }
        }

        //get keys from curve
        Keyframe[] keys = curve.keys;

        //start
        keys[0].value = 0f;

        //attack
        keys[1].time = attack / duration;
        keys[1].value = 1f;

        //sustain
        keys[2].time = (attack + sustain) / duration;
        keys[2].value = 1f;

        //decay
        keys[3].time = (attack + sustain + decay) / duration;
        keys[3].value = 0f;

        //assign back to curve
        curve.keys = keys;

        //base trigger
        base.Trigger();
    }
}

[System.Serializable]
public class Lfo
{
    public AnimationCurve curve;
    public float duration;

    private uint pos = 0;

    public void SetRandomPos(int rate, float seed)
    {
        pos = (uint)Mathf.RoundToInt(Pitch.SimpleRand(seed) * rate * duration);
    }

    public float Run(int rate)
    {
        float curvePos = Mathf.Repeat(pos / (duration * rate), 1f);

        //increment to keep track of position
        pos++;

        return curve.Evaluate(curvePos);
    }

    public void Reset()
    {
        pos = 0;
    }
}

[System.Serializable]
public class Lowpass
{
    [Range(0f, 1f)]
    public float cutoff = 0.1f;
    [Range(0f, 1f)]
    public float resonance = 0.8f;
    [Range(0f, 0.99f)]
    public float distortion = 0.25f;

    private float out1 = 0f;
    private float out2 = 0f;
    private float out3 = 0f;
    private float out4 = 0f;
    private float in1 = 0f;
    private float in2 = 0f;
    private float in3 = 0f;
    private float in4 = 0f;

    public float Run(float sampleIn)
    {
        float fc = Mathf.Clamp(cutoff, 0.0005f, 1.0f);
        float res = Mathf.Clamp(resonance, 0f, 1f);
        float input = sampleIn;
        float f = fc * 1.16f;
        float fb = res * (1.0f - 0.15f * f * f);
        input -= WaveShape(out4) * fb;
        input *= 0.35013f * (f * f) * (f * f);
        out1 = input + 0.3f * in1 + (1 - f) * out1; // Pole 1
        in1 = input;
        out2 = out1 + 0.3f * in2 + (1 - f) * out2;  // Pole 2
        in2 = out1;
        out3 = out2 + 0.3f * in3 + (1 - f) * out3;  // Pole 3
        in3 = out2;
        out4 = out3 + 0.3f * in4 + (1 - f) * out4;  // Pole 4
        in4 = WaveShape(out3);
        return out4;
    }

    private float WaveShape(float waveIn)
    {
        float x = waveIn;
        float distx = Mathf.Clamp(distortion, 0.000f, 1.0f);
        float k = 2f * distx / (1f - distx);

        return (1f + k) * x / (1f + k * Mathf.Abs(x));
    }
}

public enum Note
{
    C,
    Db,
    D,
    Eb,
    E,
    F,
    Gb,
    G,
    Ab,
    A,
    Bb,
    B
}

public static class Pitch
{
    public static float SimpleRand(float n)
    {
        float fullRand = Mathf.Sin(Vector2.Dot(new Vector2(n, 0), new Vector2(12.9898f, 78.233f))) * 43758.5453f;
        return fullRand - Mathf.Floor(fullRand);
    }

    public static Scale[] scales =
    {
        new Scale("Chromatic", new Note[] { Note.C, Note.Db, Note.D, Note.Eb, Note.E, Note.F, Note.Gb, Note.G, Note.Ab, Note.A, Note.Bb, Note.B }), //chromatic
        new Scale("Major", new Note[] { Note.C, Note.D, Note.E, Note.F, Note.G, Note.A, Note.B }), //major
        new Scale("Minor", new Note[] { Note.Db, Note.D, Note.Eb, Note.F, Note.G, Note.Ab, Note.Bb }), //minor
        new Scale("Blues", new Note[] { Note.C, Note.Eb, Note.F, Note.Gb, Note.G, Note.Bb }), //blues
        new Scale("Japanese Insen", new Note[] { Note.C, Note.Db, Note.F, Note.G, Note.Bb }) //japanese insen
    };

    public static float QuantizeFrequency(float freq)
    {
        return NotesInHertz.OrderBy(n => Mathf.Abs(n - freq)).FirstOrDefault();
    }

    public static float[] NotesInHertz = new float[]
    {
3951.066f,
3729.310f,
3520.000f,
3322.438f,
3135.963f,
2959.955f,
2793.826f,
2637.020f,
2489.016f,
2349.318f,
2217.461f,
2093.005f,
1975.533f,
1864.655f,
1760.000f,
1661.219f,
1567.982f,
1479.978f,
1396.913f,
1318.510f,
1244.508f,
1174.659f,
1108.731f,
1046.502f,
987.7666f,
932.3275f,
880.0000f,
830.6094f,
783.9909f,
739.9888f,
698.4565f,
659.2551f,
622.2540f,
587.3295f,
554.3653f,
523.2511f,
493.8833f,
466.1638f,
440.0000f,
415.3047f,
391.9954f,
369.9944f,
349.2282f,
329.6276f,
311.1270f,
293.6648f,
277.1826f,
261.6256f,
246.9417f,
233.0819f,
220.0000f,
207.6523f,
195.9977f,
184.9972f,
174.6141f,
164.8138f,
155.5635f,
146.8324f,
138.5913f,
130.8128f,
123.4708f,
116.5409f,
110.0000f,
103.8262f,
97.99886f,
92.49861f,
87.30706f,
82.40689f,
77.78175f,
73.41619f,
69.29566f,
65.40639f,
61.73541f,
58.27047f,
55.00000f,
51.91309f,
48.99943f,
46.24930f,
43.65353f,
41.20344f,
38.89087f,
36.70810f,
34.64783f,
32.70320f
    };

    public static float NoteToPitch(int note)
    {
        return NotesInHertz[Mathf.Clamp(note, 0, NotesInHertz.Length)];
    }

    public static int NotePlusOctave(Note note, int octave)
    {
        return (int)note + octave * 12;
    }
}

[System.Serializable]
public class Scale
{
    public Scale (string name, Note[] notes)
    {
        this.name = name;
        this.notes = notes;
    }

    public string name;
    public Note[] notes = new Note[0];
}