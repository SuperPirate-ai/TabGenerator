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

        notesFrequencies = new List<int>();
        for (int i = 0; i < notesSO.frequnecys.Length; i++)
        {
            notesFrequencies.Add(notesSO.frequnecys[i]);
        }

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / bufferSize;

    }

    public void Analyze(float[] _rawSamples)
    {
        //float frequency = CalculateFrequency(_rawSamples);
        float frequency = CalculateFreqeuncyWithOvertones(_rawSamples);
        if (frequency == -1) return;

        float correspondingFrequney = GetFrequencyCorespondingToNote(frequency);
        if (correspondingFrequney == 0) return;

        visualizer.Visualize(correspondingFrequney);

    }
    private float[] SNotesToBuffer(List<SNote> _notes)
    {
        float[] buffer = new float[bufferSize];
        for (int i = 0; i < _notes.Count; i++)
        {
            buffer[_notes[i].arridx] = _notes[i].volume;
        }
        return buffer;
    }
    
    private float CalculateFreqeuncyWithOvertones(float[] _samples)
    {
        //AudioComponents.Instance.ListenForEarlyReturn();
        fftBuffer = new float[bufferSize];
        
        float[] samples = _samples;
        float[] windowedSignal = AudioComponents.Instance.ApplyHannWindow(samples);

        fftBuffer = AudioComponents.Instance.FFT(windowedSignal);

        float highestValue = fftBuffer.Max();
        if (highestValue < .001f) return -1;

        float highestValueIndex = fftBuffer.ToList().IndexOf(highestValue);

        float frequencyThreshold = 250f;
        float maxFrequency = 5000f;
        float volumeThreshold = fftBuffer.Max() * .05f;


        List<SNote> overtones = CalculateOvertones(maxFrequency,volumeThreshold);

        if (overtones.Count == 0) return -1;
        float roughBaseFrequency = overtones[0].frequency;


        // get the highest overtone in relationships
        float targetFrequency = roughBaseFrequency;
        foreach (var overtone in overtones)
        {
            if (overtone.frequency < frequencyThreshold) continue;
            if (overtone.frequency > maxFrequency) continue;
            Debug.Log(overtone.frequency);
            Debug.Log(roughBaseFrequency);
            int i = 1;
            float smallestFactor = 999999;
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

        
        var vis = new Dictionary<string, object>
        {
           { "plotting_data", new List<object> {
                   new List<object> { 1, 1, fftBuffer.Take(500).ToArray()},
                   new List<object> { 1, 1, SNotesToBuffer(overtones).Take(500).ToArray()},
                   new List<object> { 1, 0, frequencyThreshold * (float)fftBuffer.Length / (float)sampleRate },
                   new List<object> { 1, 0, maxFrequency * (float)fftBuffer.Length / (float)sampleRate },
                   new List<object> { 1, 1, volumeThreshold },
               }
           }
        };
        GraphPlotter.Instance.PlotGraph(vis);
        return exactBaseFrequency;
    }


    private List<SNote> CalculateOvertones( float _maxFrequency, float _volumeThreshold)
    {
        List<SNote> overtones = new List<SNote>();
        for (int i = 3; i < Math.Clamp(fftBuffer.Length, 0, 500); i++)
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
                SNote note = new SNote();
                note.frequency = freq;
                note.arridx = i;
                note.volume = fftBuffer[i];
                overtones.Add(note);
            }
        }
        return overtones;
    }
    private float GetFreqency(int _i)
    {
        return (float)_i / (float)fftBuffer.Length * (float)sampleRate;
    }
    private float GetFrequencyCorespondingToNote(float _rawFrequency)
    {
        float corespondingFrequency = 0;

        var closestValue = notesFrequencies.Select((value, index) => new { Value = value, Index = index, Difference = Math.Abs(value - _rawFrequency) })
            .OrderBy(v => v.Difference)
            .First();

        if (Mathf.Abs(closestValue.Value - _rawFrequency) > fftError) return 0;

        corespondingFrequency = closestValue.Value;

        return corespondingFrequency;
    }

}