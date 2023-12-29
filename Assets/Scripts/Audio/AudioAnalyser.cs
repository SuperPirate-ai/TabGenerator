using NWaves.Transforms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioAnalyser : MonoBehaviour
{
    [SerializeField] int numberOfSamples = 8192;
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;
    [SerializeField] AudioVisualizer visualizer;

    private int sampleRate;
    private float[] fftReal;

    private AudioFilter audioFilter;
    private List<int> notesFrequencies;

    private float fftError;
    private float LastFreq = 0;
    private int highestBin;
   

    private void Awake()
    {
        fftReal = new float[numberOfSamples];
        
        notesFrequencies = new List<int>();
        for (int i = 0; i < notesSO.frequnecys.Length; i++)
        {
            notesFrequencies.Add(notesSO.frequnecys[i]);
        }
        
        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / numberOfSamples;
        
        highestBin = Mathf.RoundToInt(NoteManager.Instance.HighestPossibleFrequency / (float)((float)sampleRate / numberOfSamples));
        print(highestBin);
    }

    public void Analyse(AudioClip _clip)
    {
        AudioClip clip = _clip;
        float[] rawSamples = AudioComponents.Instance.ExtractDataOutOfAudioClip(clip);
        float frequency = CalculateFrequency(rawSamples);
        if (frequency == -1) return;


        float correspondingFrequney = GetFrequencyCoresbondingToNote(frequency);
        if (correspondingFrequney == 0) return;
        if (correspondingFrequney == LastFreq) return;
        
        visualizer.Visualize(correspondingFrequney);
        
        LastFreq = correspondingFrequney;
        StartCoroutine(INewNote());
    }


    private float CalculateFrequency(float[] _samples)
    {
        fftReal = new float[numberOfSamples];
        float[] samples = _samples;
        float[] highestFFTValues = new float[analysingDepth];
        int[] highestFFTBins = new int[analysingDepth];

        var fft = AudioComponents.Instance.FFT(samples);

        Array.Copy(fft, fftReal, fftReal.Length);

        //if (fftReal.Max() < 0.001f) return -1;

        float frequency = 1 /fftReal.Length * sampleRate;
        Debug.Log(frequency + " Hz");
        return frequency;
    }
    int GetClosestFrequencyBinToNote()
    {
        List<int> peakIndices = GetPeaksIndices();
        int[] sortedPeakIndices = SortIndices(peakIndices);
        int[] topIndices = new int[analysingDepth];

        Array.Copy(sortedPeakIndices,topIndices, topIndices.Length);

        float[] frequencies = new float[analysingDepth];
        for (int i = 0; i < analysingDepth; i++)
        {
            frequencies[i] = topIndices[i] / fftReal.Length * sampleRate;
        }

        var sortedFrequencies = frequencies.Select(f => f).OrderBy(f => f);
        float lowest = sortedFrequencies.First();


        return 0;
    }
    List<int> GetPeaksIndices()
    {
        List<int> peaksIndices = new List<int>();
        for (int i = 0; i < fftReal.Length; i++)
        {
            float eventualPeak;
            float left;
            float right;

            eventualPeak = fftReal[i];
            left = fftReal[i - 1];
            right = fftReal[i + 1];
            if (i == 0)
            {
                left = 0;
            }
            else if(i == fftReal.Length - 1) 
            {
                right = 0; 
            }
           
            if (left > eventualPeak || right > eventualPeak) continue;
            peaksIndices.Add(i);
        }
        return peaksIndices;
    }
    int[] SortIndices(List<int> indices)
    {
        var sorted = indices.Select(i => i).OrderByDescending(i => i);
        int[] sortedIndices = new int[indices.Count];
        int j = 0;
        foreach (int index in sorted)
        {
            sortedIndices[j++] = index;
        }
        return sortedIndices;
    }
    private float GetFrequencyCoresbondingToNote(float _rawFrequency)
    {
        float corespondingFrequency = 0;
         
        var closestValue = notesFrequencies.Select((value, index) => new { Value = value, Index = index, Difference = Math.Abs(value - _rawFrequency) })
            .OrderBy(v => v.Difference)
            .First();

        if(Mathf.Abs(closestValue.Value - _rawFrequency)> fftError) return 0;
        
        corespondingFrequency = closestValue.Value;
      
        return corespondingFrequency;
    }

    IEnumerator INewNote()
    {
        yield return new WaitForSecondsRealtime(.7f);
        LastFreq = 0;
        StopCoroutine(INewNote());
    }
}