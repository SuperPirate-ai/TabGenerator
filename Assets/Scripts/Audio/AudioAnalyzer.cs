using Accord.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Numerics;
using UnityEngine.UI;
using static UnityEngine.Awaitable;

public struct SNote
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
    [SerializeField] TMP_InputField stringInput;

    private int bufferSize;
    private int sampleRate;
    private int fftBufferLength = 8192;
    private List<int> notesFrequencies;
    private float fftError;
    private List<float> sampleHistory = new List<float>();
    private int noteStartIndexFromEnd = -1;

    private void Awake()
    {
        bufferSize = NoteManager.Instance.DefaultBufferSize;

        notesFrequencies = new List<int>(notesSO.frequnecys);

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / bufferSize;

    }

    public (float[], float[]) MainAnalyze(float[] samples)
    {
        if (noteStartIndexFromEnd != -1)
            noteStartIndexFromEnd += samples.Length;
        sampleHistory.AddRange(samples);

        (int pickstrokeIndexInSamples, bool strokeWasInPreviousSample) = AudioComponents.Instance.DetectStroke(samples.Select(x => (float)Mathf.Abs(x)).ToArray());
        (float[], float[]) features = new();

        if (pickstrokeIndexInSamples >= 0) {

            noteStartIndexFromEnd = strokeWasInPreviousSample ? samples.Length * 2 - pickstrokeIndexInSamples : samples.Length - pickstrokeIndexInSamples;
        }

        if (noteStartIndexFromEnd >= fftBufferLength)
        {
            float[] a_couple_before = sampleHistory.Skip(sampleHistory.Count() - noteStartIndexFromEnd - 8192).Take(8192).ToArray();
            float[] samplesToAnalyze = sampleHistory.Skip(sampleHistory.Count() - noteStartIndexFromEnd).Take(fftBufferLength).ToArray();
            noteStartIndexFromEnd = -1;
            float[] samplesToAnalyzePlus500 = a_couple_before.Concat(samplesToAnalyze).ToArray();
            
            var vis = new Dictionary<string, object>
            {
               { "plotting_data", new List<object> {

                        new List<object> {1,1, samplesToAnalyzePlus500},
                        new List<object> {1,0, 500},

                   }
               }
            };
            //GraphPlotter.Instance.PlotGraph(vis);
            //features = AnalyzeForTrainingData(samplesToAnalyze); //for training data
            Analyze(samplesToAnalyze);
        }

        if (sampleHistory.Count > fftBufferLength * 3)
        {
            sampleHistory = sampleHistory.Skip(sampleHistory.Count - fftBufferLength * 3).ToList();
        }

        return features;
    }

    public void Analyze(float[] _rawSamples)
    {
        // //(float[] features, float frequency,List<SNote> overtones) = CalculateExactBaseFrequencyAndFeatures(_rawSamples);
        (float frequency, float[] features, List<SNote> overtones) = GetFreq(_rawSamples);

        float correspondingFrequency = GetFrequencyCorrespondingToNote(frequency);

        if (frequency == -1 || correspondingFrequency == 0)
            return;
        if(features == null)
        {
            return;
        }
        float[] results = StringDetectionModelHandler.Instance.Predict(features);
        
        visualizer.Visualize(correspondingFrequency, results);
        //using (StreamWriter sw = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "PythonAPI", "StringAnalysis", "results", "h_E.csv"), true))
        //{
        //    sw.WriteLine("h_E," + string.Join(",", features.Select(x => x.ToString("F20"))));
        //}
        print(string.Join(",", features.Select(x => x.ToString("F20"))));
        //print("String: " + results.IndexOf(results.Max()));
        
      
    }

    public (float[],float[]) AnalyzeForTrainingData(float[] _rawSamples)
    {
        //(float[] features,float frequency,List<SNote> overtones) = CalculateExactBaseFrequencyAndFeatures(_rawSamples);
        (float frequency, float[] features, List<SNote> overtones) = GetFreq(_rawSamples);
        float correspondingFrequency = GetFrequencyCorrespondingToNote(frequency);

        if (frequency == -1 || correspondingFrequency == 0)
             return (null,null);
        
        

        //features[3] = correspondingFrequency;
        return (features,overtones.Select(x => x.frequency).ToArray());
    }
   

    private (float[], float, List<SNote>) CalculateExactBaseFrequencyAndFeatures(float[] _samples)
    {

        float[] windowedSignal = AudioComponents.Instance.ApplyHannWindow(_samples);
        float[] fftBuffer = AudioComponents.Instance.FFT(windowedSignal);
        fftBufferLength = fftBuffer.Length;
        //fftBuffer = fftBuffer.Take(fftBuffer.Length / 2).ToArray();
        //float[] frequencies = Enumerable.Range(0, _samples.Length)
        //                    .Select(i => (float)(i * NoteManager.Instance.DefaultSamplerate / _samples.Length))
        //                    .ToArray();
        //frequencies = frequencies.Take(frequencies.Length / 2).ToArray();

        //var limitIndex = Enumerable.Range(0, fftBuffer.Length)
        //                            .Where(i => frequencies[i] <= 2000f)
        //                            .ToArray();

        //fftBuffer = limitIndex.Select(i => fftBuffer[i]).ToArray();


        float highestValue = fftBuffer.Max();
        if (highestValue < .001f) return (null,-1,null);


        float frequencyThreshold = 70f;
        float maxFrequency = 5000f;
        float volumeThreshold = highestValue * .05f; //.08f


        List<SNote> overtones = CalculateOvertones(maxFrequency, volumeThreshold,fftBuffer);
        if (overtones.Count == 0) return (null, -1, null);

        

        float targetFrequency = overtones[0].frequency;
        //calculate the exact base frequency
     
        float exactBaseFrequency = overtones[0].frequency;
        foreach (var overtone in overtones)
        {
            if (overtone.frequency < frequencyThreshold || overtone.frequency > maxFrequency)
                continue;

            float baseToOvertoneFactor = (float)Math.Round(overtone.frequency / (float)exactBaseFrequency);
            exactBaseFrequency = overtone.frequency / (float)baseToOvertoneFactor;
        }
        //calculate the actual overtones
        float multipleBaseFrequency = exactBaseFrequency;
        List<float> actualOvertoneFrequencies = new();
        while (true)
        {
            foreach (var overtone in overtones)
            {
                if (Mathf.Abs(overtone.frequency - multipleBaseFrequency) <= 5) 
                {
                    actualOvertoneFrequencies.Add(overtone.frequency);
                }
            }
           
            if (multipleBaseFrequency > overtones.Max(x => x.frequency))
            {
                break;
            }
            multipleBaseFrequency += exactBaseFrequency;
        }
        
        overtones = overtones.Where(x => actualOvertoneFrequencies.Contains(x.frequency)).ToList();

        //
        Dictionary<int, float> overtoneFrequenciesADDED = new Dictionary<int, float>();
        Dictionary<int, float> overtoneAmplitudesADDED = new Dictionary<int, float>();
        foreach (var overtone in overtones)
        {
            if (overtone.frequency < frequencyThreshold || overtone.frequency > maxFrequency)
                continue;
          
            float baseToOvertoneFactor = (float)Math.Round(overtone.frequency / (float)exactBaseFrequency);
            exactBaseFrequency = overtone.frequency / (float)baseToOvertoneFactor;

            int overtoneIndex = (int)(baseToOvertoneFactor - 1);
            if (!overtoneFrequenciesADDED.ContainsKey(overtoneIndex))
            {
                overtoneFrequenciesADDED[overtoneIndex] =  overtone.frequency;
                overtoneAmplitudesADDED[overtoneIndex] = overtone.volume;
            }
            else
            {
                overtoneAmplitudesADDED[overtoneIndex] += overtone.volume;
            }

        }
      
        float avgOvertoneDiffrence = ExtractMLFeatues.Instance.CalculteOvertoneDifference(overtoneFrequenciesADDED, exactBaseFrequency);
        float ratio = ExtractMLFeatues.Instance.CalculateAmplitudeFrequencyRatio(overtones, overtoneAmplitudesADDED);
        float amplitudeRatio = ExtractMLFeatues.Instance.AplitudeRatio(overtones);
        float[] features = new float[] { ratio, amplitudeRatio, avgOvertoneDiffrence, exactBaseFrequency };


        var vis = new Dictionary<string, object>
        {
           { "plotting_data", new List<object> {

                    new List<object> {1,1, fftBuffer.Take(500).ToArray()},
                    //new List<object> {2, 1, windowedSignal.Take(500).ToArray()},
                    //new List<object> { 1, 2, envelope.Take(500).ToArray() },
                    //new List<object> { 1, 1, new List<float> {0,0}.ToArray() },

               }
           }
        };
        //GraphPlotter.Instance.PlotGraph(vis);
        return (features,exactBaseFrequency,overtones);
    }


    private List<SNote> CalculateOvertones(float _maxFrequency, float _volumeThreshold, float[] _fftBuffer)
    {
        List<SNote> overtones = new List<SNote>();
        for (int i = 3; i < Math.Min(_fftBuffer.Length,10000)-2; i++)
        {
            float freq = GetFreqency(i);
            if (_fftBuffer[i] < _volumeThreshold) continue;
            if (freq > _maxFrequency) break;
            if(freq < 70) continue;

            bool higherNearNeighbour = false;
            for (int j = Math.Max(i - 2, 0); j <= i + 2; j++)
            {
                if (j == i) continue;
                if (_fftBuffer[i] < _fftBuffer[j])
                {
                    higherNearNeighbour = true;
                    break;
                }
            }
            

            if (!higherNearNeighbour)
            {
                overtones.Add(new SNote { frequency = freq, arridx = i, volume = _fftBuffer[i] });
            }
        }
        
        return overtones;
    }
    private float GetFreqency(int _i)
    {
        return (float)_i / fftBufferLength * sampleRate;
    }
    private float GetFrequencyCorrespondingToNote(float _rawFrequency)
    {
        float closestValue = 0;
        float smallestDifference = float.MaxValue;

        foreach (var value in notesFrequencies)
        {
            float difference = Math.Abs(value - _rawFrequency);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestValue = value;
            }
        }

        if (smallestDifference > fftError)
            return 0;

        return closestValue;
    }

    private (float, float[],List<SNote>) GetFreq(float[] _samples)
    {
        //float[] windowedSignal = AudioComponents.Instance.ApplyHannWindow(_samples);
        float[] magnitudes = AudioComponents.Instance.FFTMathNet(_samples);
        
        float[] frequencies = Enumerable.Range(0, _samples.Length)
                            .Select(i => (float)(i * (float)NoteManager.Instance.DefaultSamplerate / (float)_samples.Length))
                            .ToArray();

        //using (StreamWriter sw = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "PythonAPI", "StringAnalysis", "audioSamples.csv"), true))
        //{
        //    sw.WriteLine(string.Join("\n", magnitudes.Select(x =>  x.ToString("F20").Replace(".",","))));
        //}
        // Extract positive frequencies
        int halfLength = _samples.Length / 2;
        float[] positiveFrequencies = frequencies.Take(halfLength).ToArray();
        float[] positiveMagnitudes = magnitudes.Take(halfLength).ToArray();

        // Limit to 2000 Hz
        var limitIndex = Enumerable.Range(0, positiveFrequencies.Length)
                                    .Where(i => positiveFrequencies[i] <= 2000f)
                                    .ToArray();

        float[] plotFrequencies = limitIndex.Select(i => positiveFrequencies[i]).ToArray();
        float[] plotMagnitudes = limitIndex.Select(i => positiveMagnitudes[i]).ToArray();

    
        List<SNote> overtones = new();
        for (int i = 2; i < plotMagnitudes.Length - 2; i++)
        {
            if (plotMagnitudes[i] > plotMagnitudes.Max() * 0.08f &&//0.08f
                plotMagnitudes[i] > plotMagnitudes[i - 1] &&
                plotMagnitudes[i] > plotMagnitudes[i + 1] &&
                plotMagnitudes[i] > plotMagnitudes[i - 2] + plotMagnitudes[i + 2])
            {
                //print($"Magnitude: {plotMagnitudes[i]} MagnitudeMAX {plotMagnitudes.Max()} Magnitude -1 {plotMagnitudes[i-1]} Magnitude +1 {plotMagnitudes[i +1]} Magnitude -2 {plotMagnitudes[i - 2]} Magnitude +2 {plotMagnitudes[i +2]}");
                overtones.Add(new SNote { frequency = plotFrequencies[i], volume = plotMagnitudes[i]});
            }
        }

        if (overtones.Count == 0)
        {
            return (0,null,null);
        }

        float exactBaseFrequency = overtones[0].frequency;
        Dictionary<int, float> overtoneAmplitudesADDED = new();
        Dictionary<int, List<float>> overtoneFrequenciesADDED = new();

        for (int i = 0; i < overtones.Count; i++)
        {
            float overtoneFreq = overtones[i].frequency;
            float amplitude = overtones[i].volume;

            float baseToOvertoneFactor = (float)Math.Round(overtoneFreq / exactBaseFrequency, MidpointRounding.AwayFromZero);
            exactBaseFrequency = (float)((float)overtoneFreq / (float)baseToOvertoneFactor);
            int overtoneIndex = (int)(baseToOvertoneFactor - 1);

            if (!overtoneAmplitudesADDED.ContainsKey(overtoneIndex))
            {
                overtoneAmplitudesADDED[overtoneIndex] = amplitude;
                overtoneFrequenciesADDED[overtoneIndex] = new List<float> { overtoneFreq };
            }
            else
            {
                overtoneAmplitudesADDED[overtoneIndex] += amplitude;
                overtoneFrequenciesADDED[overtoneIndex].Add(overtoneFreq);
            }
        }

        Dictionary<int, float> averagedOvertoneFrequencies = overtoneFrequenciesADDED
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Average());
        //float[] actualOvertones = new float[magnitudes.Length];
        //for (int i = 0; i < actualOvertones.Length; i++)
        //{
        //    actualOvertones[i] = -.001f;
        //}
        //for(int i = 1;i <= 5; i++)
        //{
        //    int index = (int)((exactBaseFrequency * i) / sampleRate * magnitudes.Length);
        //    actualOvertones[index] = overtones[i-1].volume;
        //}
        //var vis = new Dictionary<string, object>
        //{
        //   { "plotting_data", new List<object> {

        //            new List<object> {1, 1, actualOvertones.Take(500)},
        //            new List<object> {1,1, magnitudes.Take(500)},
        //            //new List<object> {1, 0, exactBaseFrequency*2},
        //            //new List<object> {1, 0, exactBaseFrequency*3},
        //            //new List<object> {1, 0, exactBaseFrequency*4},
        //            //new List<object> {1, 0, exactBaseFrequency*5},
        //            //new List<object> { 1, 2, envelope.Take(500).ToArray() },
        //            //new List<object> { 1, 1, new List<float> {0,0}.ToArray() },

        //       }
        //   }
        //};
        //GraphPlotter.Instance.PlotGraph(vis);

        #region
        // Compute metric 1
        //List<float> amplitudeTimesFrequencies = overtones.Select(x => x.volume).Zip(overtones.Select(x => x.frequency), (a, f) => a * f).ToList();
        //float metric1 = 1 / (amplitudeTimesFrequencies.Sum() / overtones.Count);

        //// Compute metric 2
        //float metric2 = overtoneAmplitudesADDED.GetValueOrDefault(0, 0f) - overtoneAmplitudesADDED.GetValueOrDefault(1, 1f);
        //metric2 *= 0.0001f;

        //float ratio = metric2 + metric1;
        //// Compute amplitude ratio
        //List<float> amplitudeRatios = overtones.Select(x => x.volume).Select(a => a / overtones[0].volume).ToList();
        //float amplitudeRatio = amplitudeRatios.Sum() / amplitudeRatios.Count;
        //amplitudeRatio *= 0.0001f;

        //// Compute deviation
        //float f0 = exactBaseFrequency;
        //List<float> deviations = new();

        //foreach (var kvp in averagedOvertoneFrequencies)
        //{
        //    int overtoneIndex = kvp.Key;
        //    float overtoneFreq = kvp.Value;
        //    float expectedFreq = f0 * (overtoneIndex + 1);
        //    float deviation = Math.Abs(overtoneFreq / expectedFreq);
        //    deviations.Add(deviation);
        //}

        //float avgOvertoneDiffrence = deviations.Sum() / deviations.Count;
        #endregion
        float avgOvertoneDiffrence = ExtractMLFeatues.Instance.CalculteOvertoneDifference(averagedOvertoneFrequencies, exactBaseFrequency);
        float ratio = ExtractMLFeatues.Instance.CalculateAmplitudeFrequencyRatio(overtones, overtoneAmplitudesADDED);
        float amplitudeRatio = ExtractMLFeatues.Instance.AplitudeRatio(overtones);

        float[] results = (new float[] { ratio, amplitudeRatio, avgOvertoneDiffrence, exactBaseFrequency });
        
        return (exactBaseFrequency, results, overtones);
    }

  

}