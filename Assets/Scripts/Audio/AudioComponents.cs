using Accord.Math;
using System;
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
    private float lastMedianChunkLoudness = Mathf.Infinity;
    private int lastMedianChunkLoudnessIndex = int.MinValue;
    private float penultimateMedianChunkLoudness = Mathf.Infinity;

    private const float subBufferRisingFactor = 1.70f;

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
        return DetectPickStroke(_samples, 1.70f);
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
    public (int,bool) DetectPickStroke(float[] _samples, float _subBufferRisingFactor)
    {
        float lowestFrequency = 40f;

        int chunkCount = (int)(buffersize / (NoteManager.Instance.DefaultSamplerate / lowestFrequency));

        int minimalSubBufferSize = buffersize / chunkCount;

        float[] medianChunkLoudness = new float[chunkCount];
        int[] medianChunLoudnessIndecies = new int[chunkCount];
        for (int i = 0; i < chunkCount; i++)
        {
            float[] chunk = _samples.Skip(minimalSubBufferSize * i).Take(minimalSubBufferSize).ToArray();
            medianChunkLoudness[i] = chunk.Max();
            int index = Array.IndexOf(chunk, chunk.Max());
            int realIndex = minimalSubBufferSize * i + index;
            medianChunLoudnessIndecies[i] = realIndex;
        }

        for (int i = 1; i < medianChunkLoudness.Length - 1; i++)
        {
            if (medianChunkLoudness[i] < 0.01f) continue;
            if (isPotentialAmplitudePeak(medianChunkLoudness[i - 1], medianChunkLoudness[i], _subBufferRisingFactor) && !isPotentialAmplitudePeak(medianChunkLoudness[i], medianChunkLoudness[i + 1], _subBufferRisingFactor))
            {
                //print($"picking detected with {medianChunkLoudness[i]} bigger than {medianChunkLoudness[i - 1]} times {subBufferRisingFactor}: {(medianChunkLoudness[i] * subBufferRisingFactor)}");
                return (medianChunLoudnessIndecies[i],false);
            }
        }
        if (isPotentialAmplitudePeak(lastMedianChunkLoudness, medianChunkLoudness[0], _subBufferRisingFactor) && !isPotentialAmplitudePeak(medianChunkLoudness[0], medianChunkLoudness[1], _subBufferRisingFactor))
        {
            if (medianChunkLoudness[0] > 0.01f)
            {
               // print($"picking detected with {medianChunkLoudness[0]} bigger than {lastMedianChunkLoudness} times {subBufferRisingFactor}: {(lastMedianChunkLoudness * subBufferRisingFactor)}");
                return (medianChunLoudnessIndecies[0],false);

            }
        }

        if (isPotentialAmplitudePeak(penultimateMedianChunkLoudness, lastMedianChunkLoudness, _subBufferRisingFactor) && !isPotentialAmplitudePeak(lastMedianChunkLoudness, medianChunkLoudness[0], _subBufferRisingFactor))
        {
            if (medianChunkLoudness[0] > 0.01f)
            {
                //print($"picking detected with {lastMedianChunkLoudness} bigger than {penultimateMedianChunkLoudness} times {subBufferRisingFactor}: {(penultimateMedianChunkLoudness * subBufferRisingFactor)}");
                return (lastMedianChunkLoudnessIndex,true);
            }
        }

        lastMedianChunkLoudness = medianChunkLoudness.Last();
        lastMedianChunkLoudnessIndex = medianChunLoudnessIndecies.Last();
        penultimateMedianChunkLoudness = medianChunkLoudness[medianChunkLoudness.Length - 2];

        return (int.MinValue,false);
    }

    private bool isPotentialAmplitudePeak(float _previousChuckLoudness, float _chuckLoudness, float _subBufferRisingFactor)
    {
        return _previousChuckLoudness * _subBufferRisingFactor < _chuckLoudness;
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