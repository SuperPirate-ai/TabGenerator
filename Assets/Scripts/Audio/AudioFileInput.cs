using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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
}
