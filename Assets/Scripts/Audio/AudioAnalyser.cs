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
        print(sampleRate);
    }

    public void Analyse(AudioClip _clip)
    {
        print(sampleRate);
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

        //taking Filter
        audioFilter = new AudioFilter(75, 1000, fftReal);
        fftReal = ApplyFilter(fftReal);
        //fftReal = audioFilter.HighPassFilter(75, fftReal);

        SortedDictionary<int,float> peaks = GetSortedHighestFFTPeaks(analysingDepth);


        int j = analysingDepth;
        foreach (var item in peaks)
        {
            j--;

            if (item.Value < .0001f) { highestFFTValues[j] = -1; continue; }

            highestFFTValues[j] = item.Value;
            highestFFTBins[j] = item.Key;
        }


        int bestPeakBin = GetBestPeakBinBasedOnProtrusion(highestFFTValues, highestFFTBins);
        if (bestPeakBin == -1) return -1;

        float frequency = bestPeakBin * sampleRate /fftReal.Length;
        //for (int i = 0; i < highestFFTValues.Length; i++)
        //{
        //    if (highestFFTValues[i] == -1) { frequencies[i] = -1; continue; }
        //    frequencies[i] = (highestFFTBins[i] * (sampleRate) / fftReal.Length);
        //}

        return frequency;
    }
    private SortedDictionary<int,float> GetSortedHighestFFTPeaks(int _numberOfPeaks)
    {
        #region taking peaks of fftReal
        var values = fftReal.Select((value, index) => new { Value = value, Index = index });
        var sortedValues = values.OrderByDescending(item => item.Value);
        var highestValues = sortedValues.Take(_numberOfPeaks);
        #endregion

        SortedDictionary<int, float> sortedPeaks = new SortedDictionary<int, float>();

        foreach (var value in highestValues)
        {
            sortedPeaks.Add(value.Index, value.Value);
        }

        return sortedPeaks;
    }
    private float[] ApplyFilter(float[] _frequencies)
    {
        float[] frequencies = _frequencies;
        NWaves.Signals.DiscreteSignal sig = new NWaves.Signals.DiscreteSignal(sampleRate, frequencies);
        var movingFilter = new  NWaves.Filters.MovingAverageFilter(5);
        var smoothSig = movingFilter.ApplyTo(sig);

        return smoothSig.Samples;
    }
    private int GetBestPeakBinBasedOnProtrusion(float[] _peaks, int[] _bins)
    {
        float[] protrusionValues = new float[_peaks.Length];
        for (int i = 0; i < _peaks.Length; i++)
        {
            protrusionValues[i] = CalculateProtrusionValue(_peaks[i], _bins[i]);
        }
        var maxIndex = protrusionValues.Select((value, index) => new { Value = value, Index = index }).OrderByDescending(item => item.Value).First().Index;
        if (_peaks[maxIndex] == -1) return -1;
        return _bins[maxIndex];
    }
    private float CalculateProtrusionValue(float _peak,int _bin)
    {
        float protrusionValue;
        if(_bin == fftReal.Length - 1)
        {
            protrusionValue = _peak/ fftReal[_bin -1];
        }
        else if(_bin == 0)
        {
            protrusionValue = _peak / fftReal[_bin + 1];
        }
        else
        {
            protrusionValue = _peak / Mathf.Max((float)fftReal[_bin - 1], (float)fftReal[_bin + 1]);
        }
        return protrusionValue;
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