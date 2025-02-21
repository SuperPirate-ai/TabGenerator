using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

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
        return (float)Math.Round((decimal)(amplitudesRatio.Average() * .0001f), 20);
    }


    public float CalculteOvertoneDifference(Dictionary<int,float> _overtoneFrequencies, float _fundamentalFrequency)
    {
        List<float> overtoneDifferences = new();
        foreach(var overtone in _overtoneFrequencies)
        {
            float expectedFrequency = (float)((float)_fundamentalFrequency * (overtone.Key + 1));
            overtoneDifferences.Add(Mathf.Abs((float)((float)overtone.Value/(float)expectedFrequency)));
        }
        if (overtoneDifferences.Count == 0) return 0;
        return (float)Math.Round((decimal)overtoneDifferences.Average(),20);
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
        float avgRatio = (float)(1/(float)ratio / (float)amplitudes.Length);//metric_1



        int ampLength = amplitudes.Length;
        float metric_2 = (0 <  ampLength? amplitudes[0]: 0f) - (1 < ampLength ? amplitudes[1]: 1f);

        metric_2 *= .0001f;

        return (float)Math.Round((decimal)((float)avgRatio + (float)metric_2),20);
    }
}

