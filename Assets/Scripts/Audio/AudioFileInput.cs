using UnityEngine;
using System;
using PlasticPipe.Certificates;
using System.Collections.Generic;
using System.IO;

public class AudioFileInput : MonoBehaviour
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioAnalyzer analyser;
    
    public void StartAnalysingBtn()
    {
        float[] samples = AudioComponents.Instance.ExtractDataOutOfAudioClip(audioClip, 0);
        List<float[]> features = new List<float[]>();
        for (int i = 0; i < samples.Length; i+= NoteManager.Instance.DefaultBufferSize)//only predict for one buffer and print the results for every step
        {
            float[] subbuffer = new float[NoteManager.Instance.DefaultBufferSize];
            Array.Copy(samples,i,subbuffer,0, NoteManager.Instance.DefaultBufferSize);
            features.Add(analyser.Analyze(subbuffer));
        }
        print($"Predicted value0: {features[0]}");

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "features.csv");
        print(Directory.GetCurrentDirectory());
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (float[] feature in features)
            {
                string line = string.Join(",", feature);
                writer.WriteLine(line);
            }
        }
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
