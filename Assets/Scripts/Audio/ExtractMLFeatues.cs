using Accord.Statistics;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExtractMLFeatues : MonoBehaviour
{
    public static ExtractMLFeatues Instance;

    private void Awake()
    {
        Instance = this;
    }
    public float AplitudeRatio(List<SNote> _overtones)
    {
        float[] amplitudesRatio = new float[_overtones.Count];
        for (int i = 0; i < _overtones.Count; i++)
        {
            amplitudesRatio[i] = _overtones[i].volume / _overtones[0].volume;
        }
        return amplitudesRatio.Average() * .0001f;

    }


    public float CalculteOvertoneDifference(Dictionary<int,float> _overtoneFrequencies, float _fundamentalFrequency)
    {
        float[] overtoneDifferences = new float[_overtoneFrequencies.Count];
        foreach(var overtone in _overtoneFrequencies)
        {
            float expectedFrequency = _fundamentalFrequency * (overtone.Key +1);
            overtoneDifferences[overtone.Key] = Mathf.Abs(overtone.Value/expectedFrequency);
        }
        return overtoneDifferences.Average();
    }

    public float CalculateAmplitudeFrequencyRatio(List<SNote> _overtones)
    {
        float[] amplitudes = _overtones.Select(x => x.volume).ToArray();
        float[] frequencies = _overtones.Select(x => x.frequency).ToArray();

        float ratio = 0;

      

        for (int i = 0; i < amplitudes.Length; i++)
        {
            ratio += amplitudes[i] * frequencies[i];
        }
        float avgRatio = 1/ratio / amplitudes.Length;//metric_1



        int ampLength = amplitudes.Length;
        float metric_2 = (0 <  ampLength? amplitudes[0]: 0) - (1< ampLength? amplitudes[1]: 1);

        metric_2 *= .0001f;

        return avgRatio + metric_2;
    }
}

