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
        float thresholdFactor = .15f;
        AudioComponents.Instance.ListenForEarlyReturn();

        fftBuffer = new float[numberOfSamples];
        float[] samples = _samples;
        fftBuffer = AudioComponents.Instance.FFT(samples);

        List<(float,int)> allovertones = CalculateOvertones(thresholdFactor);
        if (allovertones == null) return -1;

        List<float> overtones = allovertones.Select(x => x.Item1).ToList();
        List<int> overtonesIndecies = allovertones.Select(x => x.Item2).ToList();

        
        List<float> overtoneFreq = new List<float>();
        string freqOutput = "";
        for (int i = 0; i < overtones.Count; i++)
        {
            float freq = (float)overtonesIndecies[i] / (float)fftBuffer.Length * (float)sampleRate;
            //if(freq > 1300 || freq < 150) continue;
            overtoneFreq.Add(freq);
            freqOutput += $"{overtoneFreq.Count - 1}. overtone: " + overtoneFreq.Last() + " ";
        }

        Debug.Log(freqOutput + " COUNT: " + overtones.Count);

        float avgDistance = 0;
        int overtoneCount = overtoneFreq.Count;
        for (int i = 0; i < overtoneFreq.Count - 1; i++)
        {
            if(overtoneFreq[i] < 150)
            {
                overtoneCount--;
                continue;
            }
            avgDistance += overtoneFreq[i + 1] - overtoneFreq[i];
        }
        avgDistance /= overtoneCount - 1;
        
        Debug.Log("avgDistance: " + avgDistance);


         
        

        if (!AudioComponents.Instance.DetectPickStroke(samples) || avgDistance < 70)
        {
            return -1;
        }


        //Plotting Graph


        DateTime currentTime = DateTime.Now;
        var vis = new Dictionary<string, object>
        {
            { "plotting_data", new List<object> {
                    new List<object> { 1, 1, fftBuffer.Take(500).ToArray()},new List<object> { 1, 1, fftBuffer.Max() *thresholdFactor},
                    new List<object> { 2, 1, overtones.ToArray()}

                }
            }
        };
        GraphPlotter.Instance.PlotGraph(vis);


        //
        return avgDistance; 
    }

    private List<(float,int)> CalculateOvertones(float _threasholdFactor)
    {

        float highestValue = fftBuffer.Max();
        if (highestValue < .001f) return null;

        List<float> overtones = new List<float>();
        List<int> overtonesIndecies = new List<int>();

        float threashold = _threasholdFactor * highestValue;

        for (int i = 1; i < fftBuffer.Length - 1; i++)
        {
            bool isOverThreshold = fftBuffer[i] > threashold;
            bool isLocalHigh = fftBuffer[i] > fftBuffer[i - 1] && fftBuffer[i] > fftBuffer[i + 1];
            float freq = GetFreqency(i);
            bool isInImportantSpectrum = freq < 10000;
            
            bool isValidPeak = isOverThreshold && isLocalHigh && isInImportantSpectrum;

            if (isValidPeak)
            {
                overtones.Add(fftBuffer[i]);
                overtonesIndecies.Add(i);
            }
        }


       

        return overtones.Select((value, index) => (value, overtonesIndecies[index])).ToList();
    }
    private float GetFreqency(int _i)
    {
        return (float)_i / (float)fftBuffer.Length * (float)sampleRate;
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