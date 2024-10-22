using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct SNote
{
    public float frequency;
    public float volume;
    public int arridx;
    public string name;
}

public class AudioAnalyzer : MonoBehaviour
{
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;
    [SerializeField] AudioVisualizer visualizer;

    private int bufferSize;
    private int sampleRate;
    private float[] fftBuffer;
    private List<int> notesFrequencies;
    private float fftError;

    private void Awake()
    {
        bufferSize = NoteManager.Instance.DefaultBufferSize;
        fftBuffer = new float[bufferSize];

        notesFrequencies = new List<int>(notesSO.frequnecys);

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / bufferSize;

    }

    public void Analyze(float[] _rawSamples)
    {
        float frequency = CalculateFrequencyWithOvertones(_rawSamples);
        float correspondingFrequency = GetFrequencyCorrespondingToNote(frequency);

        if (frequency == -1 || correspondingFrequency == 0 || !AudioComponents.Instance.NewNoteDetected(correspondingFrequency, _rawSamples))
            return;

        visualizer.Visualize(correspondingFrequency);

    }
    private float[] SNotesToBuffer(List<SNote> _notes)
    {
        float[] buffer = new float[bufferSize];
        foreach (var note in _notes)
        {
            buffer[note.arridx] = note.volume;
        }
        return buffer;
    }

    private float CalculateFrequencyWithOvertones(float[] _samples)
    {
        Array.Clear(fftBuffer, 0, bufferSize);

        float[] windowedSignal = AudioComponents.Instance.ApplyHannWindow(_samples);
        fftBuffer = AudioComponents.Instance.FFT(windowedSignal);

        float highestValue = fftBuffer.Max();
        if (highestValue < .001f) return -1;


        float frequencyThreshold = 250f;
        float maxFrequency = 5000f;
        float volumeThreshold = highestValue * .05f;


        List<SNote> overtones = CalculateOvertones(maxFrequency, volumeThreshold);
        if (overtones.Count == 0) return -1;

        float roughBaseFrequency = overtones[0].frequency;
        float targetFrequency = roughBaseFrequency;

        foreach (var overtone in overtones)
        {
            if (overtone.frequency < frequencyThreshold || overtone.frequency > maxFrequency)
                continue;

            int i = 1;
            float smallestFactor = float.MaxValue;
            while (true)
            {
                float newFactor = (overtone.frequency / i) / targetFrequency;
                if (newFactor < 1)
                {
                    newFactor = 1 / newFactor;
                }
                if (newFactor < smallestFactor)
                {
                    smallestFactor = newFactor;
                    i++;
                    continue;
                }
                i--;
                break;
            }
            targetFrequency = overtone.frequency / i;
        }

        float exactBaseFrequency = targetFrequency;
        float[] envelope = AudioComponents.Instance.CalculateEnvelope(windowedSignal, bufferSize / 18);

        //List<(float[], int[], bool)> deriv = AudioComponents.Instance.DetectPickStrokeV2(samples);
        //float[] hops = deriv[0].Item1;
        //int[] derivX = deriv[0].Item2;
        //bool isStroke = deriv[0].Item3;

        //if (!isStroke)
        //    exactBaseFrequency = -1;

        //print("IS STROKE");
        //float[] maxsInBuffer = new float[bufferSize];
        //for (int i = 0; i < hops.Length; i++)
        //{
        //    maxsInBuffer[derivX[i]] = hops[i];
        //}



        var vis = new Dictionary<string, object>
        {
           { "plotting_data", new List<object> {
                    //new List<object> {1,2,maxsInBuffer.ToArray()},
                   // new List<object> {1,1, fftBuffer.Take(500).ToArray()},
                    new List<object> {1,1, windowedSignal.ToArray()},
                    new List<object> { 1, 2, envelope.ToArray() },
                    new List<object> { 1, 1, new List<float> {0,0}.ToArray() },

               }
           }
        };
        GraphPlotter.Instance.PlotGraph(vis);
        return exactBaseFrequency;
    }


    private List<SNote> CalculateOvertones(float _maxFrequency, float _volumeThreshold)
    {
        List<SNote> overtones = new List<SNote>();
        for (int i = 3; i < Math.Min(fftBuffer.Length, 500); i++)
        {
            float freq = GetFreqency(i);
            if (fftBuffer[i] < _volumeThreshold) continue;
            if (freq > _maxFrequency) break;

            bool higherNearNeighbour = false;
            for (int j = Math.Max(i - 2, 0); j <= i + 2; j++)
            {
                if (j == i) continue;
                if (fftBuffer[i] < fftBuffer[j])
                {
                    higherNearNeighbour = true;
                    break;
                }
            }

            if (!higherNearNeighbour)
            {
                overtones.Add(new SNote { frequency = freq, arridx = i, volume = fftBuffer[i] });
            }
        }
        return overtones;
    }
    private float GetFreqency(int _i)
    {
        return (float)_i / fftBuffer.Length * sampleRate;
    }
    private float GetFrequencyCorrespondingToNote(float _rawFrequency)
    {
        float closestValue = 0;
        float smallestDifference = float.MaxValue;

        foreach (var value in notesFrequencies)
        {
            float difference = Math.Abs(value - _rawFrequency);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestValue = value;
            }
        }

        if (smallestDifference > fftError)
            return 0;

        return closestValue;
    }

}