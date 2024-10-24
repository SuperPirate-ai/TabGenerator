using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class AudioComponents : MonoBehaviour
{
    private int buffersize;
    public static AudioComponents Instance;
    private float lastPeakLoudness = 0;
    public int earlyReturnCounter = 0;
    private float lastNoteFrequency = 1;
    private float lastNoteAmplitude = -1;
    private float lastMedianChunkLoudness = Mathf.Infinity;
    private const float subBufferRisingFactor = 1.75f;

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
            float hannValue = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (N - 1)));
            windowedSignal[i] = signal[i] * hannValue;
        }

        return windowedSignal;
    }
    int buffersUntilNextPosibleStroke = 0;
    public bool NewNoteDetected(float _noteFrequency, float[] _samples)
    {
        if(buffersUntilNextPosibleStroke > 0)
        {
            buffersUntilNextPosibleStroke--;
            return false;
        }
        if (FrequencyChange(_noteFrequency) || DetectPickStroke(_samples))
        {
            buffersUntilNextPosibleStroke = 2;
            return true;
        }
        
        return false;
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
            print("Frequency change");
            return true;
        }
        return false;
    }
    public bool DetectPickStroke(float[] _samples)
    {
        float lowestFrequency = 40f;

        int chunkCount = (int)(buffersize / (NoteManager.Instance.DefaultSamplerate / lowestFrequency));

        int minimalSubBufferSize = buffersize / chunkCount;

        float[] medianChunkLoudness = new float[chunkCount];
        for (int i = 0; i < chunkCount; i++)
        {
            float[] chunk = _samples.Skip(minimalSubBufferSize * i).Take(minimalSubBufferSize).ToArray();
            medianChunkLoudness[i] = chunk.Max();
        }

        bool isStroke = false;
        for (int i = 0; i < medianChunkLoudness.Length - 1; i++)
        {
            if (medianChunkLoudness[i] * subBufferRisingFactor < medianChunkLoudness[i + 1])
            {
                if(medianChunkLoudness[i + 1] < 0.01f) continue;
                print($"picking detected with {medianChunkLoudness[i +1]} bigger than {medianChunkLoudness[i]} times {subBufferRisingFactor}: {(medianChunkLoudness[i] * subBufferRisingFactor)}");
                isStroke = true;
            }
        }
        if (lastMedianChunkLoudness * subBufferRisingFactor < medianChunkLoudness[0])
        {
            if (medianChunkLoudness[0] > 0.01f)
            {
                print($"picking detected with {medianChunkLoudness[0]} bigger than {lastMedianChunkLoudness} times {subBufferRisingFactor}: {(lastMedianChunkLoudness * subBufferRisingFactor)}" );
                isStroke = true;
            }
        }
        lastMedianChunkLoudness = medianChunkLoudness.Last();
        
       return isStroke;
    }

    public bool DetectPickStrokeRichard1(float[] _samples)
    {
        float lowestFrequency = 60f;

        float[] envelope = CalculateEnvelope(_samples, buffersize / 18);
        int chunkCount = (int)(buffersize / (NoteManager.Instance.DefaultSamplerate / lowestFrequency));

        int minimalSubBufferSize = buffersize / chunkCount;

        float[] medianChunkLoudness = new float[chunkCount];
        for (int i = 0; i < chunkCount; i++)
        {
            float[] chunk = envelope.Skip(minimalSubBufferSize * i).Take(minimalSubBufferSize).ToArray();
            float[] gradients = ComputeDerivative(chunk, 1);

            List<float> peaks = new List<float>();
            for (int j = 0; j < gradients.Length; j++)
            {
                if (gradients[j] > 0 && gradients[j + 1] < 0)
                {
                    peaks.Add(chunk[j + 1]);
                }
            }

            medianChunkLoudness[i] = peaks.Average();
        }
        bool isStroke = false;
        for (int i = 0; i < medianChunkLoudness.Length - 1; i++)
        {
            if (Mathf.Pow(medianChunkLoudness[i], 2) * 1.5f < Mathf.Pow(medianChunkLoudness[i + 1], 2))
            {
                Debug.Log($"picking detected at {i + 1}");
                isStroke = true;
            }
        }
        if (Mathf.Pow(lastMedianChunkLoudness, 2) * 1.5f < Mathf.Pow(medianChunkLoudness[0], 2))
        {
            isStroke = true;
        }
        lastMedianChunkLoudness = medianChunkLoudness.Last();

        return isStroke;
    }


    public List<(float[], int[], bool)> DetectPickStrokeV2(float[] _samples)
    {
        float deltaX = 1;

        float[] samples = new float[_samples.Length + 1];

        float[] envelope = CalculateEnvelope(_samples, buffersize / 18);
        if (lastPeakLoudness == 0)
        {
            samples = envelope;
        }
        else
        {
            samples[0] = lastPeakLoudness;
            Array.Copy(envelope, 0, samples, 1, envelope.Length);
        }

        float[] derivative = ComputeDerivative(samples, deltaX);

        List<float> maxPoints = new List<float>();
        List<int> pointX = new List<int>();
        for (int i = 1; i < derivative.Length; i++)
        {
            if (derivative[i - 1] > 0 && derivative[i] < 0)
            {
                maxPoints.Add((float)samples[i]);
                pointX.Add(i);
            }

        }
        bool isStroke = false;
        for (int i = 0; i < maxPoints.Count - 1; i++)
        {
            if (maxPoints[i] * 1.5f < maxPoints[i + 1]/* && maxPoints[i] *3 < maxPoints[i+2]*/)
            {
                print("------------");
                print(maxPoints[i] + "|" + maxPoints[i + 1] /*"|" + maxPoints[i+2]*/);
                isStroke = true;
                break;
            }

        }
        return new List<(float[], int[], bool)> { (maxPoints.ToArray(), pointX.ToArray(), isStroke) };
    }


    public float[] CalculateEnvelope(float[] inputSignal, int smoothingWindow)
    {
        int length = inputSignal.Length;
        float[] envelope = new float[length];

        // Step 1: Full-wave rectification (absolute value of the signal)
        float[] rectifiedSignal = inputSignal.Select(x => Math.Abs(x)).ToArray();

        // Step 2: Smoothing the rectified signal using a simple moving average (SMA)
        for (int i = 0; i < length; i++)
        {
            float sum = 0;
            int count = 0;

            // Smoothing window
            for (int j = i; j < Math.Min(i + smoothingWindow, length); j++)
            {
                sum += rectifiedSignal[j];
                count++;
            }

            envelope[i] = (float)sum / count; // Simple moving average for smoothing
        }

        return envelope;
    }
    float[] ComputeDerivative(float[] _samples, float _deltaX)
    {
        int sampleLength = _samples.Length;
        float[] derivative = new float[sampleLength];

        for (int i = 0; i < sampleLength - 1; i++)
        {
            derivative[i] = (float)(_samples[i + 1] - _samples[i]) / (float)_deltaX;
        }

        lastPeakLoudness = derivative[sampleLength - 1];

        return derivative;
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