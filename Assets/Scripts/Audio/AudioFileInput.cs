using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class AudioFileInput : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClips;
    [SerializeField] AudioClip[] audioClipsTEST;
    [SerializeField] AudioAnalyzer analyzer;

    List<string[]> features = new List<string[]>();
    public void StartAnalysingBtn()
    {
        foreach (AudioClip audioClip in audioClipsTEST)
        {
            NoteManager.Instance.DefaultSamplerate = audioClip.frequency;
            string fileName = audioClip.name;
            string stringName = fileName.Split("_str")[0];
            float[] audioClipData = AudioComponents.Instance.ExtractAllDataOutOfAudioClip(audioClip, 0);
            List<float[]> subSamples = new List<float[]>();

            for (int i = 0; i < audioClipData.Length; i += NoteManager.Instance.DefaultBufferSize)
            {
                int subSampleLength = Math.Min(NoteManager.Instance.DefaultBufferSize, audioClipData.Length - i);
                float[] subbuffer = new float[subSampleLength];
                Array.Copy(audioClipData, i, subbuffer, 0, subSampleLength);
                subSamples.Add(subbuffer);
            }
            foreach(var suB in subSamples)
            {
                if(suB.Length < 8192)
                    continue;
                (float[] newfeatures, _) = analyzer.MainAnalyze(suB);
                if (newfeatures == null)
                    continue;
                string[] featuresString = newfeatures.Select(x => x.ToString("F20")).ToArray();
                string[] featuresWithSTRINGNAME = new string[] { stringName }.Concat(featuresString).ToArray();
                features.Add(featuresWithSTRINGNAME);
            }


        }
        features.RemoveAll(x => x == null);

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "PythonAPI", "StringAnalysis", "results", "Testfeatures.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string[] feature in features)
            {
                string line = string.Join(",", feature);
                writer.WriteLine(line);
            }
        }
        Debug.Log("Features saved to " + filePath);
    }

    private bool Analyze(float[] previousBuffer, float[] subb,int pickStrokeIndex,string stringName)
    {
        float[] combinedBuffer = new float[previousBuffer.Length - pickStrokeIndex];
        Array.Copy(previousBuffer, pickStrokeIndex, combinedBuffer, 0, previousBuffer.Length - pickStrokeIndex);
        Array.Resize(ref combinedBuffer, combinedBuffer.Length + pickStrokeIndex);
        Array.Copy(subb, 0, combinedBuffer, pickStrokeIndex, Math.Min(subb.Length, 8192 - pickStrokeIndex));


        (float[] analyzedFeatures, float[] overtones) = analyzer.AnalyzeForTrainingData(combinedBuffer);
        if (analyzedFeatures == null)
            return false;
        string[] overtoneSTRING = overtones.Select(x => x.ToString("F20")).ToArray();
        string[] analyzedFeaturesString = analyzedFeatures.Select(x => x.ToString("F20")).ToArray();
        string[] featuresWithSTRINGNAME = new string[] { stringName }.Concat(analyzedFeaturesString).ToArray();
        features.Add(featuresWithSTRINGNAME);
        return true;
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
