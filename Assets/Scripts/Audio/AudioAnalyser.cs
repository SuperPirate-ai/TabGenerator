using System;
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

    private List<int> notesFrequencies;

    private float fftError;

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

    }

    public void Analyse(float[] _rawSamples)
    {
        float frequency = CalculateFrequency(_rawSamples);
        if (frequency == -1) return;

        float correspondingFrequney = GetFrequencyCoresbondingToNote(frequency);
        if (correspondingFrequney == 0) return;

        visualizer.Visualize(correspondingFrequney);

    }


    private float CalculateFrequency(float[] _samples)
    {
        AudioComponents.Instance.ListenForEarlyReturn();
        fftReal = new float[numberOfSamples];
        float[] samples = _samples;

        var fft = AudioComponents.Instance.FFT(samples);

        Array.Copy(fft, fftReal, fftReal.Length);

        float lowestFFTValue = 1;
        float highestFFTValue = 0;
        int highestFFTBin = 0;
        for (int i = 0; i < fftReal.Length; i++)
        {
            if (fftReal[i] > highestFFTValue)
            {
                highestFFTValue = fftReal[i];
                highestFFTBin = i;
            }

            if (fftReal[i] < lowestFFTValue)
            {
                lowestFFTValue = fftReal[i];
            }
        }

        //frequency Calculation
        float frequency = (float)highestFFTBin / (float)fftReal.Length * (float)sampleRate;
        //peak gate

        if (!AudioComponents.Instance.DetectPickStroke(samples, frequency)) return -1;


        // octave Detection
        float octaveFreq = AudioComponents.Instance.DetectOctaveInterference(frequency, fftReal, highestFFTBin);
        if (octaveFreq != frequency)
        {
            frequency = octaveFreq;
        }
        // noise gate
        if (highestFFTValue < .001f) return -1;

        return frequency;
    }


    private float GetFrequencyCoresbondingToNote(float _rawFrequency)
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