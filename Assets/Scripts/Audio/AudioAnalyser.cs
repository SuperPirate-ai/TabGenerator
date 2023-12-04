using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

public class AudioAnalyser : MonoBehaviour
{
    [SerializeField] int numberOfSmaples = 8192;
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;

    private int sampleRate;
    private double[] fftReal;

    private AudioVisualizer visualizer;
    private AudioFilter audioFilter;
    private List<int> notesFrequencies;

    private float fftError;
    private float LastFreq = 0;

    private void Awake()
    {
        fftReal = new double[numberOfSmaples];
        notesFrequencies = new List<int>();
        for (int i = 0; i < notesSO.frequnecys.Length; i++)
        {
            notesFrequencies.Add(notesSO.frequnecys[i]);
        }

        visualizer = new AudioVisualizer();
        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / numberOfSmaples;
    }

    public void Analyse(AudioClip _clip)
    {
        double[] rawSamples = AudioComponents.Instance.ExtractDataOutOfAudioClip(_clip);
        float[] frequencies = CalculateFrequencies(rawSamples);
       
        float[] correspondingFrequneys = GetFrequenciesCoresbondingToNote(frequencies);
        if (correspondingFrequneys.Length == 0) return;
        if (correspondingFrequneys[0] == LastFreq) return;

        visualizer.Visualize(correspondingFrequneys[0]);
      
        LastFreq = correspondingFrequneys[0];
        StartCoroutine(INewNote());
    }

    private float[] CalculateFrequencies(double[] _samples)
    {
        double[] samples = _samples;
        double[] highestFFTValues = new double[analysingDepth];
        int[] highestFFTBins = new int[analysingDepth];

        var fft = AudioComponents.Instance.FFT(samples);
        Array.Copy(fft, fftReal, fftReal.Length);

        //taking higpassFilter
        audioFilter = new AudioFilter(75, 1000, fftReal);
        //fftReal = audioFilter.HighPassFilter(75, fftReal);

        SortedDictionary<int,double> peaks = GetSortedHighestFFTPeaks(analysingDepth);


        int j = analysingDepth;
        foreach (var item in peaks)
        {
            j--;

            if (item.Value < .0001f) { highestFFTValues[j] = -1; continue; }

            highestFFTValues[j] = item.Value;
            highestFFTBins[j] = item.Key;
        }

        float[] frequencies = new float[analysingDepth];
        for (int i = 0; i < highestFFTValues.Length; i++)
        {
            if (highestFFTValues[i] == -1) { frequencies[i] = -1; continue; }
            frequencies[i] = (highestFFTBins[i] * (sampleRate / 2) / fftReal.Length)/4;
        }

        return frequencies;
    }
    private SortedDictionary<int,double> GetSortedHighestFFTPeaks(int _numberOfPeaks)
    {
        #region taking peaks of fftReal
        var values = fftReal.Select((value, index) => new { Value = value, Index = index });
        var sortedValues = values.OrderByDescending(item => item.Value);
        var highestValues = sortedValues.Take(_numberOfPeaks);
        #endregion

        SortedDictionary<int, double> sortedPeaks = new SortedDictionary<int, double>();

        foreach (var value in highestValues)
        {
            sortedPeaks.Add(value.Index, value.Value);
        }

        return sortedPeaks;
    }
    private float[] GetFrequenciesCoresbondingToNote(float[] _rawFrequencies)
    {
        float[] rawFrequencies = Array.FindAll(_rawFrequencies, x => x != -1);

        float[] corespondingFrequencies = new float[rawFrequencies.Length];

        for (int i = 0; i < rawFrequencies.Length; i++)
        {
            float rawFreq = rawFrequencies[i];

            var closestValue = notesFrequencies.Select((value, index) => new { Value = value, Index = index, Difference = Math.Abs(value - rawFreq) })
                .OrderBy(v => v.Difference)
                .First();
            corespondingFrequencies[i] = closestValue.Value;
        }

        float[] coreespondingFreqNoZeros = Array.FindAll(corespondingFrequencies, x => x != 0);

        return coreespondingFreqNoZeros;
    }

    IEnumerator INewNote()
    {
        yield return new WaitForSecondsRealtime(.7f);
        LastFreq = 0;
        StopCoroutine(INewNote());
    }
}