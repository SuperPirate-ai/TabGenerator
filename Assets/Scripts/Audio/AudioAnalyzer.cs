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
        //float frequency = CalculateFrequency(_rawSamples);
        float frequency = CalculateFreqeuncyWithOvertones(_rawSamples);
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
                        {"FFT", fftBuffer.Take(420).ToList().ToArray()  },
                        
                    }
                }
            };
        GraphPlotter.Instance.PlotGraph(vis);


        return frequency;
    }


    private float CalculateFreqeuncyWithOvertones(float[] _samples)
    {
        AudioComponents.Instance.ListenForEarlyReturn();
        fftBuffer = new float[numberOfSamples];
        float[] samples = _samples;
        fftBuffer = AudioComponents.Instance.FFT(samples);

        float avgAmplitude = fftBuffer.Average();
        //float medianAmplitude = 0;

        //float[] fftBufferSorting = new float[fftBuffer.Length];
        //Array.Copy(fftBuffer, fftBufferSorting, fftBuffer.Length);
        //Array.Sort(fftBufferSorting);
        //if (fftBufferSorting.Length % 2 == 0)
        //{
        //    medianAmplitude = (fftBufferSorting[fftBuffer.Length / 2] + fftBufferSorting[fftBuffer.Length / 2 - 1]) / 2;
        //}
        //else
        //{
        //    medianAmplitude = fftBufferSorting[fftBuffer.Length / 2];
        //}



        float highestValue = fftBuffer.Max();
        if (highestValue < .001f) return -1;

        float highestValueIndex = fftBuffer.ToList().IndexOf(highestValue);
        List<float> overtones = new List<float>();
        List<int> overtonesIndecies = new List<int>();
        float threashold = 0.05f * highestValue;

        for (int i = 1; i < fftBuffer.Length -1; i++)
        {
            bool isOverAverage = fftBuffer[i] > threashold;
            bool isLocalHigh =  fftBuffer[i] > fftBuffer[i - 1]  && fftBuffer[i] > fftBuffer[i + 1];
            

            bool isValidPeak = isOverAverage && isLocalHigh;

            if (isValidPeak)
            {
                overtones.Add(fftBuffer[i]);
                overtonesIndecies.Add(i);
            }
        }

        float fundamentalfreq = (float)overtonesIndecies[0] / (float)fftBuffer.Length * (float)sampleRate;
        float firstovertone = (float)overtonesIndecies[1] / (float)fftBuffer.Length * (float)sampleRate;
        float secondovertone = (float)overtonesIndecies[2] / (float)fftBuffer.Length * (float)sampleRate;
        
        Debug.Log("highestValfreq: " + fundamentalfreq + " 1.overtonefreq: " + firstovertone + " 2.OvertoneFreq: " + secondovertone + " COUNT: " + overtones.Count);


        DateTime currentTime = DateTime.Now;
        var vis = new Dictionary<string, object>
        {
            { "plotting_data", new List<object> {
                    new List<object> { 1, 1, fftBuffer.Take(500).ToArray()},new List<object> { 1, 1, threashold},
                    new List<object> { 2, 1, overtones.ToArray().Reverse()}

                }
            }
        };
        GraphPlotter.Instance.PlotGraph(vis);
        //print(overtones.Count());

        int averageDistanceBetweenOvertones = 0;
        for (int i = 0; i < overtones.Count - 1; i++)
        {
            averageDistanceBetweenOvertones += overtonesIndecies[i + 1] - overtonesIndecies[i];
        }
        if(overtones.Count == 0) return -1;
        averageDistanceBetweenOvertones /= overtones.Count - 1;


        for (int i = 0; i < overtones.Count; i++)
        {
            float freqeuncy = (float)(overtonesIndecies[i] / (float)(fftBuffer.Length * (float)sampleRate));
            if (freqeuncy > 150)
            {
                float fundamentalFreqValue = (float)overtonesIndecies[i] - averageDistanceBetweenOvertones * i;
                float fundamentalFreq = fundamentalFreqValue / (float)fftBuffer.Length * (float)sampleRate;
                if (!AudioComponents.Instance.DetectPickStroke(samples, fundamentalFreq)) return -1;
                Debug.Log("Frequency: " + fundamentalFreq);
                return fundamentalFreq;
            }
        }
        return 0;
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