using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    [SerializeField] int numberOfSamples = 8192;
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;
    [SerializeField] AudioVisualizer visualizer;

    private int sampleRate;
    private float[] fftBuffer;

    private List<int> notesFrequencies;

    private float fftError;

    private void Awake()
    {
        fftBuffer = new float[numberOfSamples];

        notesFrequencies = new List<int>();
        for (int i = 0; i < notesSO.frequnecys.Length; i++)
        {
            notesFrequencies.Add(notesSO.frequnecys[i]);
        }

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / numberOfSamples;

    }

    public void Analyze(float[] _rawSamples)
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
        fftBuffer = new float[numberOfSamples];
        float[] samples = _samples;

        fftBuffer = AudioComponents.Instance.FFT(samples);


        float lowestFFTValue = 1;
        float highestFFTValue = 0;
        int highestFFTBin = 0;
        for (int i = 0; i < fftBuffer.Length; i++)
        {
            if (fftBuffer[i] > highestFFTValue)
            {
                highestFFTValue = fftBuffer[i];
                highestFFTBin = i;
            }

            if (fftBuffer[i] < lowestFFTValue)
            {
                lowestFFTValue = fftBuffer[i];
            }
        }

        //frequency Calculation
        float frequency = (float)highestFFTBin / (float)fftBuffer.Length * (float)sampleRate;
        //peak gate

        if (!AudioComponents.Instance.DetectPickStroke(samples, frequency)) return -1;


        // octave Detection
        float octaveFreq = AudioComponents.Instance.DetectOctaveInterference(frequency, fftBuffer, highestFFTBin);
        if (octaveFreq != frequency)
        {
            frequency = octaveFreq;
        }
        // noise gate
        if (highestFFTValue < .001f) return -1;

        DateTime currentTime = DateTime.Now;
        var vis = new Dictionary<string, object>
            {
                { "time" , currentTime.ToString("HH:mm:ss")},
                { "common_scaling_groups", new string[][] {
                        new string [] { "samples" },
                        new string [] { "FFT" }
                    }
                },
                { "y_data_s", new Dictionary <string, object> {
                        {"samples",samples },
                        {"FFT", fftBuffer.Take(420).ToList().ToArray()  }
                    
                    }
                }
            };
        GraphPlotter.Instance.PlotGraph(vis);


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