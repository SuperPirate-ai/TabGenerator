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
    private float fftError;
    private AudioFilter audioFilter;


    private List<int> notesFrequencys;

    float LastFreq = 0;

    private void Awake()
    {
        fftReal = new double[numberOfSmaples];
        notesFrequencys = new List<int>();
        for (int i = 0; i < notesSO.frequnecys.Length; i++)
        {
            notesFrequencys.Add(notesSO.frequnecys[i]);
        }

        sampleRate = NoteManager.Instance.defaultSamplerate;
        fftError = sampleRate / numberOfSmaples;
    }

    public void Visualize(AudioClip _clip)
    {
        double[] rawSamples = AudioComponents.Instance.ExtractDataOutOfAudioClip(_clip);
        float[] frequencys = CalculateFrequency(rawSamples);

        float[] corresbondingFrequneys = GetFrequencysCoresbondingToNote(frequencys);
        if (corresbondingFrequneys.Length == 0) return;
        if (corresbondingFrequneys[0] == LastFreq) return;

        Vector3[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(corresbondingFrequneys[0]);
        NoteManager.Instance.InstantiateNotes(notePos);

        LastFreq = corresbondingFrequneys[0];
        StartCoroutine(INewNote());
    }

    private float[] CalculateFrequency(double[] _samples)
    {
        double[] samples = _samples;

        var fft = AudioComponents.Instance.FFT(samples);
        Array.Copy(fft, fftReal, fftReal.Length);

        //taking higpassFilter
        audioFilter = new AudioFilter(75, 1000, fftReal);
        //fftReal = audioFilter.HighPassFilter(75, fftReal);

        float[] frequencys = GetHighestFFTPeaks(analysingDepth);
        return frequencys;
    }
    private float[] GetHighestFFTPeaks(int _numberOfPeaks)
    {
        double[] highestFFTValues = new double[_numberOfPeaks];
        int[] highestFFTBins = new int[_numberOfPeaks];

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


        int j = _numberOfPeaks;
        foreach (var item in sortedPeaks)
        {
            print(item.Value + "||" + item.Key);
            j--;

            if (item.Value < .0001f) { highestFFTValues[j] = -1; continue; }

            highestFFTValues[j] = item.Value;
            highestFFTBins[j] = item.Key;
        }

        float[] frequencys = new float[_numberOfPeaks];
        for (int i = 0; i < highestFFTValues.Length; i++)
        {
            if (highestFFTValues[i] == -1) { frequencys[i] = -1; continue; }
            frequencys[i] = (highestFFTBins[i] * (sampleRate / 2) / fftReal.Length) / 2;
        }
        return frequencys;
    }
    private float[] GetFrequencysCoresbondingToNote(float[] _rawFrequencys)
    {
        float[] corespondingFrequencys = new float[_rawFrequencys.Length];

        for (int i = 0; i < notesFrequencys.Count; i++)
        {
            float noteFreq = notesFrequencys[i];
            for (int a = 0; a < _rawFrequencys.Length; a++)
            {
                float rawFreq = _rawFrequencys[a];

                bool newFreqIsCloserToNoteFreq = Mathf.Abs(noteFreq - rawFreq) < Mathf.Abs(noteFreq - corespondingFrequencys[a]);
                bool isInFFtError = Mathf.Abs(noteFreq - rawFreq) < fftError;

                if (newFreqIsCloserToNoteFreq && isInFFtError)
                {
                    corespondingFrequencys[a] = noteFreq;
                }
            }
        }

        float[] coreespondingFreqNoZeros = Array.FindAll(corespondingFrequencys, x => x != 0);

        return coreespondingFreqNoZeros;
    }

    IEnumerator INewNote()
    {
        yield return new WaitForSecondsRealtime(.7f);
        LastFreq = 0;
        StopCoroutine(INewNote());
    }
}