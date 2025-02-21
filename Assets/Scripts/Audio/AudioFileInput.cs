using Accord.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Numerics;

public class AudioFileInput : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClips;
    [SerializeField] AudioAnalyzer analyser;

    public void StartAnalysingBtn()
    {
        List<string[]> features = new List<string[]>();
        foreach (AudioClip audioClip in audioClips)
        {
            string fileName = audioClip.name;
            string stringName = fileName.Split("_str")[0];
            float[] samples = AudioComponents.Instance.ExtractAllDataOutOfAudioClip(audioClip, 0);
            for (int i = 0; i < samples.Length; i += NoteManager.Instance.DefaultBufferSize)//only predict for one buffer and print the results for every step
            {
                if (i + NoteManager.Instance.DefaultBufferSize > samples.Length)
                {
                    break;
                }
                float[] subbuffer = new float[NoteManager.Instance.DefaultBufferSize];
                Array.Copy(samples, i, subbuffer, 0, NoteManager.Instance.DefaultBufferSize);
                float[] analyzedFeatures = analyser.AnalyzeForTrainingData(subbuffer);
                if (analyzedFeatures == null)
                    continue;
               
                string[] analyzedFeaturesString = analyzedFeatures.Select(x => x.ToString("F20")).ToArray();
                features.Add(new string[] { stringName }.Concat(analyzedFeaturesString).ToArray());

            }
        }
        features.RemoveAll(x => x == null);
        //sort features with the 4th element of the array
        //features.Sort((x, y) => x[4].CompareTo(y[4]));

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "PythonAPI", "StringAnalysis", "results", "features.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string[] feature in features)
            {
                string line = string.Join(",", feature);
                writer.WriteLine(line);
            }
        }
        Debug.Log("Features saved to " + filePath);

        #region
        //int halfWaveLengths = (int)Mathf.Floor(NoteManager.Instance.DefaultSamplerate / 100);
        //List<float> loundesses = new List<float>();
        //for (int i = 0; i < samples.Length; i += halfWaveLengths)
        //{
        //    float[] subbuffer = new float[halfWaveLengths];
        //    Array.Copy(samples, i, subbuffer, 0, halfWaveLengths);
        //    float maxValue = 0f;
        //    for (int j = 0; j < subbuffer.Length; j++)
        //    {
        //        float value = Mathf.Abs(subbuffer[j]);
        //        maxValue = value > maxValue ? value: maxValue;
        //    }
        //    loundesses.Add(maxValue);
        //}

        //for (int i = 0; i < loundesses.Count; i++)
        //{
        //    if(loundesses[i] > 0.02f && loundesses[i-1] * 2.5f < loundesses[i])
        //    {
        //    }
        //}

        // analyser.Analyze(samples);
        #endregion
    }

    public void TestFeatures()
    {
    
        float exactBaseFrequency = 110.357666015625f;
        List<SNote> overtones = new List<SNote>();
        

        Dictionary<int, float> overtoneAmplitudes = new Dictionary<int, float>()
        {
            { 0, 15.70545916621923f },
            { 1, 39.27227199061098f },
            { 2, 41.03155114471267f },
            { 3, 25.54529816602438f },
            { 4, 13.570982669067005f },
            { 6, 11.944206822079572f },
            { 7, 9.039757866689385f },
            { 8, 10.469554669106007f },
            { 9, 4.840042586973985f }
        };

        Dictionary<int, float> overtoneFrequencies = new Dictionary<int, float>()
        {
            { 0, 107.666015625f },
            { 1, 220.71533203125f },
            { 2, 328.38134765625f },
            { 3, 441.4306640625f },
            { 4, 549.0966796875f },
            { 6, 769.81201171875f },
            { 7, 882.861328125f },
            { 8, 990.52734375f },
            { 9, 1103.57666015625f }
        };

        foreach (var (overtoneIndex, amplitude) in overtoneAmplitudes)
        {
            float frequency = overtoneFrequencies.GetValueOrDefault(overtoneIndex, 0);
            overtones.Add(new SNote { volume = amplitude, frequency = frequency });
        }

        Debug.Log(string.Join(",", overtones.Select(x => x.frequency)));

        Dictionary<int, List<float>> overtoneFrequenciesADDED = new Dictionary<int, List<float>>();
        Dictionary<int, float> overtoneAmplitudeADDED = new Dictionary<int, float>();
        foreach (var overtone in overtones)
        {
            if (overtone.frequency < 250f || overtone.frequency > 5000f)
                continue;

            float baseToOvertoneFactor = (float)Math.Round(overtone.frequency / exactBaseFrequency);

            int overtoneIndex = (int)(baseToOvertoneFactor - 1);

            if (!overtoneAmplitudeADDED.ContainsKey(overtoneIndex))
            {
                overtoneAmplitudeADDED[overtoneIndex] = overtone.volume;
                overtoneFrequenciesADDED[overtoneIndex] = new List<float> { overtone.frequency };
            }
            else
            {
                overtoneAmplitudeADDED[overtoneIndex] += overtone.volume;
                overtoneFrequenciesADDED[overtoneIndex].Add(overtone.frequency);
            }
        }

        Dictionary<int, float> overtoneFrequenciesADDEDAverage = new Dictionary<int, float>();
        foreach (var overtoneFrequencyADDED in overtoneFrequenciesADDED)
        {
            overtoneFrequenciesADDEDAverage.Add(overtoneFrequencyADDED.Key, overtoneFrequencyADDED.Value.Average());
        }

        float metric2 = ExtractMLFeatues.Instance.CalculateAmplitudeFrequencyRatio(overtones,overtoneAmplitudeADDED);
        float deviation = ExtractMLFeatues.Instance.CalculteOvertoneDifference(overtoneFrequenciesADDEDAverage, exactBaseFrequency);
        float ampRatio = ExtractMLFeatues.Instance.AplitudeRatio(overtones);

        print($"Features\n BaseFrequency: {exactBaseFrequency} Metric2: {metric2} AmplitudeRatio: {ampRatio} OvertoneDiffrence: {deviation} ");



    }


}
