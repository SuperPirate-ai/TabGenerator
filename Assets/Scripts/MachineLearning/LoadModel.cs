using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;

public class LoadModel : MonoBehaviour
{
    public ModelAsset modelAsset;
    private Model runtimeModel;
    void Start()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MLModels/modelData");
        string csvText = csvFile.text;

        string[] lines = csvText.Split('\n');

        (string, float, float, float) data = new();

        foreach (string line in lines)
        {
            string[] columns = line.Split(',');

        }



            runtimeModel = ModelLoader.Load(modelAsset);
        Tensor<float> input = new Tensor<float>(new TensorShape(4),a);

    }
}
