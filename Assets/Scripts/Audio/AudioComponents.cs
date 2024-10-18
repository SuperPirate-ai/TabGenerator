using System.Linq;
using UnityEngine;
using System.Numerics;
using System.Collections.Generic;
using System;
using Accord.Math;
using static UnityEditor.ShaderData;
using System.Runtime.InteropServices.WindowsRuntime;

public class AudioComponents : MonoBehaviour
{
    private int buffersize;
    public static AudioComponents Instance;
    private float lastSubsampleLoudnessOfPreviousBuffer = Mathf.Infinity;
    private float lastPeakLoudness = 0;
    public int earlyReturnCounter = 0;

    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }

    private void Start()
    {
        buffersize = NoteManager.Instance.DefaultBufferSize;

        DetectPichStrokeV2(new float[0]);
    }
    private void Update()
    {
        if (NoteManager.Instance.PlayPaused)
        {
            lastSubsampleLoudnessOfPreviousBuffer = Mathf.Infinity;
        }
    }
    public float[] ExtractDataOutOfAudioClip(AudioClip _clip, int _positionInClip)
    {
        float[] samples = new float[buffersize];

        _clip.GetData(samples, _positionInClip);

        return samples;
    }
    public float[] ApplyHannWindow(float[] signal)
    {
        int N = signal.Length;
        float[] windowedSignal = new float[N];

        for (int i = 0; i < N; i++)
        {
            // Hann window formula
            float hannValue = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (N - 1)));
            windowedSignal[i] = signal[i] * hannValue;
        }

        return windowedSignal;
    }

    public bool DetectPickStroke(float[] _samples, float _frequency = 0)
    {
        earlyReturnCounter = 0;
        int subsamples = /*_frequency > 150 ?*/ 64 /*: 16*/; // => needs to be a power of 2 || 128 -> 2^7
        int subsampleSize = _samples.Length / subsamples;
        float[] subsampleLoudnesses = new float[subsamples];


        for (int i = 0; i < subsampleLoudnesses.Length; i++)
        {
            for (int j = i * subsampleSize; j < i * subsampleSize + subsampleSize; j++)
            {
                if (Mathf.Abs(_samples[j]) > subsampleLoudnesses[i])
                {
                    subsampleLoudnesses[i] = Mathf.Abs(_samples[j]);
                }
            }
        }

        bool stroke = false;

        for (int i = 0; i < subsampleLoudnesses.Length - 1; i++)
        {

            if (subsampleLoudnesses[i] * 2f < subsampleLoudnesses[i + 1])
            {
                stroke = true;
                break;
            }
        }
        if (lastSubsampleLoudnessOfPreviousBuffer * 2f < subsampleLoudnesses[0])
        {
            stroke = true;

        }

        lastSubsampleLoudnessOfPreviousBuffer = subsampleLoudnesses.Last();
        return stroke;
    }
    bool DetectPichStrokeV2(float[] _samples)
    {
        float deltaX = 1 / (float)NoteManager.Instance.DefaultSamplerate;

        float[] samples = new float[_samples.Length + 1];
        if(lastPeakLoudness == 0)
        {
            samples = _samples;
        }
        else
        { 
            samples[0] = lastPeakLoudness;
            Array.Copy(_samples, 0, samples, 1, _samples.Length);
        }


        // Calculate the derivative using finite differences
        double[] derivative = new double[_samples.Length];
        for (int i = 1; i < _samples.Length - 1; i++)
        {
            derivative[i] = (_samples[i + 1] - _samples[i - 1]) / deltaX;
        }

        List<float> maxPoints = new List<float>();
        // Find the maximum points (where derivative changes sign from positive to negative)
        for (int i = 0; i < derivative.Length - 1; i++)
        {
            if (derivative[i] > 0 && derivative[i + 1] < 0)
            {
                maxPoints.Add( (float)derivative[i]);
            }
            
        }
        if(maxPoints.Average() < 0.1f)
        {
            return false;
        }

        for (int i = 0; i < maxPoints.Count; i++)
        {
            if (maxPoints[i] * 2f < maxPoints[i + 1])
            {
                return true;
            }
        }
        //for(int i = 0; i < 100000000; i++)
        //{
        //    var vis = new Dictionary<string, object>
        //    {
        //        { "plotting_data", new List<object> {
        //                new List<object> { 1, 1, derivative}


        //            }
        //        }
        //    };
        //    GraphPlotter.Instance.PlotGraph(vis);
        //}
        return false;
    }
    float[] ComputeDerivative(float[] _samples, float _deltaX)
    {
        int sampleLength = _samples.Length;
        float[] derivative = new float[sampleLength];

        // Compute central difference for interior points
        for (int i = 0; i < sampleLength - 1; i++)
        {
            //print($"sample i+1: {_samples[i+1]} samples i: {_samples[i]} result: {(_samples[i + 1] - _samples[i]) / _deltaX}");
            derivative[i] = (float)(_samples[i+1] - _samples[i])/ (float)_deltaX;
        }



        lastPeakLoudness = derivative[sampleLength - 1];

        return derivative;
    }
    public void ListenForEarlyReturn()
    {
        if (earlyReturnCounter > 0)
        {
            lastSubsampleLoudnessOfPreviousBuffer = 0;
        }
        earlyReturnCounter++;
    }
    public float DetectOctaveInterference(float _frequency, float[] _fftReal, int _freqBin)
    {
        float lowerOctaveFrequency = _frequency / 2;
        int lowerOctaveBin = (int)((float)lowerOctaveFrequency / (float)NoteManager.Instance.DefaultSamplerate * (float)_fftReal.Length);

        float lowerOctaveAmplitude = _fftReal[lowerOctaveBin];
        float freqAmplitudedReduced = _fftReal[_freqBin] * 0.60f;

        if (freqAmplitudedReduced < lowerOctaveAmplitude)
        {
            return lowerOctaveFrequency;
        }
        return _frequency;

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
}
