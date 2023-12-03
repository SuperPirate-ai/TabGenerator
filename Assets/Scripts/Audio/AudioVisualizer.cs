using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [SerializeField] int numberOfSmaples = 8192;
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;

    private int sampleRate;
    private double[] fftReal;


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

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / numberOfSmaples;
    }

    public void Visualize(AudioClip _clip)
    {
        double[] rawSamples = AudioComponents.Instance.ExtractDataOutOfAudioClip(_clip);
        float[] frequencies = CalculateFrequencies(rawSamples);
       
        float[] corresbondingFrequneys = GetFrequenciesCoresbondingToNote(frequencies);
        if (corresbondingFrequneys.Length == 0) return;
        if (corresbondingFrequneys[0] == LastFreq) return;
       
        print(frequencies[0]+ " corresponse to " +corresbondingFrequneys[0]);
        Vector3[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(corresbondingFrequneys[0]);
        NoteManager.Instance.InstantiateNotes(notePos);

        LastFreq = corresbondingFrequneys[0];
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
            frequencies[i] = (highestFFTBins[i] * (sampleRate / 2) / fftReal.Length);
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
        float[] corespondingFrequencies = new float[_rawFrequencies.Length];

        for (int i = 0; i < notesFrequencies.Count; i++)
        {
            float noteFreq = notesFrequencies[i];
            for (int a = 0; a < _rawFrequencies.Length; a++)
            {
                float rawFreq = _rawFrequencies[a];

                bool newFreqIsCloserToNoteFreq = Mathf.Abs(noteFreq - rawFreq) < Mathf.Abs(noteFreq - corespondingFrequencies[a]);
                bool isInFFtError = Mathf.Abs(noteFreq - rawFreq) < fftError;

                if (newFreqIsCloserToNoteFreq && isInFFtError)
                {
                    corespondingFrequencies[a] = noteFreq;
                }
            }
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