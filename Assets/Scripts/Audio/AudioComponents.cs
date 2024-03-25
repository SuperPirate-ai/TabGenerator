using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

public class AudioComponents : MonoBehaviour
{
    [SerializeField] NotesSO notes;
    private int buffersize;
    public static AudioComponents Instance;
    private float previousMaxDisplacement = Mathf.Infinity;
    private float previousFrequency;

    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }

    private void Start()
    {
        buffersize = NoteManager.Instance.DefaultBufferSize;
    }
    private void Update()
    {
        if (NoteManager.Instance.PlayPaused)
        {
            previousMaxDisplacement = Mathf.Infinity;
        }
    }
    public float[] ExtractDataOutOfAudioClip(AudioClip _clip, int _positionInClip)
    {
        float[] sample = new float[buffersize];
        _clip.GetData(sample, _positionInClip);
        return sample;
    }
    public bool DetectPickStroke(float[] _sample,float _frequency)
    {
        float maxDisplacement = 0;
        for (int i = 0; i < _sample.Length; i++)
        {
            if (Math.Abs(_sample[i]) > maxDisplacement) {
                maxDisplacement = Math.Abs(_sample[i]);
            }
        }

        int getNoteIndex(float _frequency)
        {
            float frequencyDistanceFromLastOne = float.MaxValue;
            for (int i = 0; i < notes.frequnecys.Length; i++)
            {
                if (Math.Abs(notes.frequnecys[i] - _frequency) > frequencyDistanceFromLastOne)
                {
                    return i;
                }
                frequencyDistanceFromLastOne = Math.Abs(notes.frequnecys[i] - _frequency);
            }
            return -1;
        }

        int nowidx = getNoteIndex(_frequency);
        int thenidx = getNoteIndex(previousFrequency);

        if (maxDisplacement > previousMaxDisplacement * 1.3 || (nowidx != thenidx)) {
            previousMaxDisplacement = maxDisplacement;
            previousFrequency = _frequency;
            return true;
        }
        previousMaxDisplacement = maxDisplacement;
        previousFrequency = _frequency;
        return false;
    }

    public float DetectOctaveInterference(float _frequnecy, float[] _fftReal, int _freqBin)
    {
        float lowerOctaveFrequency = _frequnecy / 2;
        int lowerOctaveBin = (int)((float)lowerOctaveFrequency / (float)NoteManager.Instance.DefaultSamplerate * (float)_fftReal.Length);

        float lowerOctaveAmplitude = _fftReal[lowerOctaveBin];
        float freqAmplitudedReduced = _fftReal[_freqBin] * 0.60f;

        if (freqAmplitudedReduced < lowerOctaveAmplitude)
        {
            return lowerOctaveFrequency;
        }
        return _frequnecy;

    }
    public float[] FFT(float[] _data)
    {
        float[] fft = new float[_data.Length];
        System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new System.Numerics.Complex(_data[i], 0.0);
        }

        Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = (float)fftComplex[i].Magnitude;
        }

        return fft;
    }
}
