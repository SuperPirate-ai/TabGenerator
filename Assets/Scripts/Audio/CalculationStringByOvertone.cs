using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Accord.Statistics;
public class CalculationStringByOvertone : MonoBehaviour
{
    public static CalculationStringByOvertone Instance;

    private void Awake()
    {
        Instance = this;
    }

    
    public float Calculate_B_AverageValue(float[] _overtones)
    {
        float fundamentalFrequency = _overtones[0];
        float[] bValues = new float[_overtones.Length];

        for (int i = 0; i < _overtones.Length; i++)
        {
            bValues[i] = Calculate_B_Value(fundamentalFrequency, _overtones[i], i + 1 );
          // print("B Value: "+ bValues[i]);
        }
        Debug.Log($"Standard: {bValues.StandardDeviation()}");
        return bValues.Average();
    }
    private float Calculate_B_Value(float _fundamentalFrequency, float _overtoneFrequency, int _index)
    {
        return (float)((float)Mathf.Pow((float)_overtoneFrequency/(float)(_fundamentalFrequency * _index), 2)-1)/(float)Mathf.Pow(_index,2);
    }

 
}

