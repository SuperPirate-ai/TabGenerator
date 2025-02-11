using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using System.IO;
using static UnityEngine.UI.GridLayoutGroup;
using System.Reflection.Emit;

public class LoadModel : MonoBehaviour
{
    public ModelAsset modelAsset;
    private Model runtimeModel;
    void Start()
    {
        string path = Path.Combine(Application.dataPath, "MachineLearning", "TestData", "testresults2.csv");
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
        (string label1, float[] features1) = dataList[0];
        for (int i = 0; i < features1.Length; i++)
        {
            print(features1[i]);
        }

        runtimeModel = ModelLoader.Load(modelAsset);
        //print inputshape
        Debug.Log($"Input shape: {runtimeModel.inputs[0].shape}");
        Worker worker = new Worker(runtimeModel, BackendType.CPU);

        string[] strings = { "h_E", "B", "G", "D", "A", "E" };

        int predictedright = 0;
        int predictedwrong = 0;
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
                //Debug.Log($"<color=green>Expected: {label}, Got: {predictedLabel}</color>");
                predictedright++;
            else
                predictedwrong++;
                //Debug.Log($"<color=red>Expected: {label}, Got: {predictedLabel}</color>");
                inputTensor.Dispose();
                outputTensor.Dispose();


          
        }

        worker.Dispose(); // Clean up
        Debug.Log($"Predicted right: {predictedright}");
        Debug.Log($"Predicted wrong: {predictedwrong}");
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
