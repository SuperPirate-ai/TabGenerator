using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Accord.Audio;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;


#if UNITY_EDITOR
using NUnit.Framework;
#endif

public class AudioComponents : MonoBehaviour
{
    private int buffersize;
    public static AudioComponents Instance;
    private float lastPeakLoudness = 0;
    public int earlyReturnCounter = 0;
    private float lastNoteFrequency = 1;
    private float lastMedianChunkLoudness = Mathf.Infinity;
    private int lastMedianChunkLoudnessIndex = int.MinValue;
    private float penultimateMedianChunkLoudness = Mathf.Infinity;


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
            //lastSubsampleLoudnessOfPreviousBuffer = Mathf.Infinity;
            lastMedianChunkLoudness = Mathf.Infinity;
            penultimateMedianChunkLoudness = Mathf.Infinity;
        }
    }
    public float[] ExtractDataOutOfAudioClip(AudioClip _clip, int _positionInClip)
    {
        float[] samples = new float[buffersize];
        _clip.GetData(samples, _positionInClip);
        return samples;
    }
    public float[] ExtractAllDataOutOfAudioClip(AudioClip _clip, int _positionInClip)
    {
        float[] samples = new float[_clip.samples];
        _clip.GetData(samples, _positionInClip);
        return samples;
    }
    public float[] ApplyHannWindow(float[] signal)
    {
        int N = signal.Length;
        float[] windowedSignal = new float[N];

        for (int i = 0; i < N; i++)
        {
            float hannValue = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (N - 1)));
            windowedSignal[i] = signal[i] * hannValue;
        }

        return windowedSignal;
    }
    //public bool NewNoteDetected(float _noteFrequency, float[] _samples)
    //{
    //    bool hasPickStroke = DetectPickStroke(_samples, 1.80f);//1.70f
    //    bool hasFrequencyChange = FrequencyChange(_noteFrequency) && DetectPickStroke(_samples, 1.50f);
    //    if (/*hasFrequencyChange ||*/ hasPickStroke)
    //    {
    //        return true;
    //    }

    //    return false;
    //}
    public (int,bool) DetectStroke(float[] _samples)
    {
        return DetectPickStroke(_samples, 2f, 0.01f);
    }
    private bool FrequencyChange(float _noteFrequency)
    {
        const float frequencyChangeThreshold = 0.95f;
        float frequencyFactor = lastNoteFrequency / _noteFrequency;
        if (frequencyFactor > 1)
        {
            frequencyFactor = 1 / frequencyFactor;
        }
        if (frequencyChangeThreshold > frequencyFactor)
        {
            lastNoteFrequency = _noteFrequency;
            //print("Frequency change");
            return true;
        }
        return false;
    }

    private float[] previous_pickstrokedetection_samples;
    private const float lowestFrequency = 80f;
    public (int,bool) DetectPickStroke(float[] _samples, float _subBufferRisingFactor, float threshold)
    {
        if (previous_pickstrokedetection_samples == null)
        {
            previous_pickstrokedetection_samples = _samples;
            return (int.MinValue, false);
        }

        int minimalSubBufferSize = (int)(NoteManager.Instance.DefaultSamplerate / lowestFrequency);

#if UNITY_EDITOR
        Assert.IsTrue(minimalSubBufferSize < _samples.Length, "Minimal sub buffer size is less than sample length");
#endif


        float[] last_and_this_sample = previous_pickstrokedetection_samples.Concat(_samples).ToArray();
        

        float[] loudnesses = new float[(int)(last_and_this_sample.Length / minimalSubBufferSize)];
        for (int i = 0; i < loudnesses.Length; i++)
        {
            for (int j = i * minimalSubBufferSize; j < (i + 1) * minimalSubBufferSize; j++)
            {
                if (loudnesses[i] < last_and_this_sample[j])
                {
                    loudnesses[i] = last_and_this_sample[j];
                }
            }
        }

        var vis = new Dictionary<string, object>
        {
           { "plotting_data", new List<object> {

                    new List<object> {1,1, loudnesses},
                    new List<object> {1,1, threshold}

               }
           }
        };
        //GraphPlotter.Instance.PlotGraph(vis);


        int exactPickstrokeIndex = int.MinValue;
        bool peak_loudness_in_previous_samples = false;


        for (int i = 1; i < loudnesses.Length; i++)
        {
            if (loudnesses[i - 1] * _subBufferRisingFactor < loudnesses[i] && threshold < loudnesses[i])
            {
                previous_pickstrokedetection_samples = _samples;
                peak_loudness_in_previous_samples = i * minimalSubBufferSize < previous_pickstrokedetection_samples.Length;

                int startIndex = i * minimalSubBufferSize;
                int endIndex = (i + 1) * minimalSubBufferSize;

                int maxIndex = 0;
                for (int j = startIndex; j < endIndex; j++)
                {
                    if (last_and_this_sample[j] > last_and_this_sample[maxIndex])
                    {
                        maxIndex = j;
                    }
                }

                exactPickstrokeIndex = maxIndex;
                break;
            }
        }
        previous_pickstrokedetection_samples = _samples;

        return (exactPickstrokeIndex, peak_loudness_in_previous_samples);
    }

 
    public float[] FFT(float[] _data)
    {
        float[] fft = new float[_data.Length];
        Complex[] fftComplex = new Complex[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new Complex(_data[i], 0.0);
        }

        Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = (float)fftComplex[i].Magnitude;
        }

        return fft;
    }
    public float[] FFTMathNet(float[] _data)
    {
        //float[] hannWindow = Window.Hann(_data.Length).Select(x => (float)x).ToArray();
        //for (int i = 0; i < _data.Length; i++)
        //    _data[i] *= hannWindow[i];
        float[] fft = new float[_data.Length];
        Complex32[] fftComplex = new Complex32[_data.Length];
        for (int i = 0; i < _data.Length; i++)
        {
            fftComplex[i] = new Complex32(_data[i], 0.0f);
        }
        MathNet.Numerics.IntegralTransforms.Fourier.Forward(fftComplex,FourierOptions.AsymmetricScaling);
        for (int i = 0; i < _data.Length; i++)
        {
            fft[i] = (float)fftComplex[i].Magnitude;
        }
        return fft;

    }


}