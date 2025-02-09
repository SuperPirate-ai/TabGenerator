using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using System.IO;
using static UnityEngine.UI.GridLayoutGroup;

public class LoadModel : MonoBehaviour
{
    public ModelAsset modelAsset;
    private Model runtimeModel;
    void Start()
    {
        string path = Path.Combine(Application.dataPath, "MachineLearning", "TestData", "testresults.csv");
        path = path.Replace("\\", "/");
        Debug.Log($"Path: {path}");
        string csvText = File.ReadAllText(path);

        string[] lines = csvText.Split('\n');

        List<(string, float[] features)> dataList = new List<(string, float[])>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] columns = line.Split(',');

            if (columns.Length < 5) continue; // Ensure correct format

            string label = columns[0];
            float[] features = new float[columns.Length -1];
            for (int i = 1; i < columns.Length; i++)
            {
                if (float.TryParse(columns[i], out float value))
                    features[i - 1] = value;
            }
            dataList.Add((label, features));
        }

        runtimeModel = ModelLoader.Load(modelAsset);
        //print inputshape
        Debug.Log($"Input shape: {runtimeModel.inputs[0].shape}");
        Worker worker = new Worker(runtimeModel, BackendType.GPUCompute);

        string[] strings = { "h_E", "B", "G", "D", "A", "E" };


        foreach (var (label, features) in dataList)
        {
            Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, features.Length), features);
            worker.Schedule(inputTensor);
            Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;

            if (outputTensor == null)
            {
                Debug.LogError("Model inference failed.");
                continue;
            }

            float[] outputValues = outputTensor.DownloadToArray();
            int predictedIndex = ArgMax(outputValues);
            string predictedLabel = strings[predictedIndex];

            if (label == predictedLabel)
                Debug.Log($"<color=green>Expected: {label}, Got: {predictedLabel}</color>");
            else
                Debug.Log($"<color=red>Expected: {label}, Got: {predictedLabel}</color>");

          
        }

        worker.Dispose(); // Clean up
    }
    private int ArgMax(float[] values)
    {
        int bestIndex = 0;
        float bestValue = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > bestValue)
            {
                bestValue = values[i];
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}
