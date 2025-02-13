using Accord.Audio;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Linq;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Rendering;

public class LoadModel : MonoBehaviour
{
    public ModelAsset modelAsset;
    private Model runtimeModel;
    void Start()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;




        string path = Path.Combine(Application.dataPath, "MachineLearning", "TestData", "testresults2.csv");
        path = path.Replace("\\", "/");
        string csvText = File.ReadAllText(path);

        string[] lines = csvText.Split('\n');

        List<(string, float[] features)> dataList = new List<(string, float[])>();

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] columns = line.Split(',');

            if (columns.Length < 5) continue; // Ensure correct format
            string label = columns[0];
            float[] features = new float[columns.Length - 1];
            for (int i = 1; i < columns.Length; i++)
            {
                features[i - 1] = float.Parse(columns[i],CultureInfo.InvariantCulture.NumberFormat);
            }
            // print("featuresBEFORE: " + string.Join(",", features.Select(x => x.ToString("F8"))));
            dataList.Add((label, features));
        }

        runtimeModel = ModelLoader.Load(modelAsset);
       
        //print inputshape
        Worker worker = new Worker(runtimeModel, BackendType.CPU);

        string[] strings = { "h_E", "B", "G", "D", "A", "E" };

        int predictedright = 0;
        int predictedwrong = 0;

        foreach (var (label, features) in dataList)
        {
            float min = features.Min();
            float max = features.Max();
            float[] normalized = features.Select(x => (x - min) / (max - min)).ToArray();
            Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, normalized.Length), normalized);
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


        (string label0, float[] features0) = dataList[56];

        print("features: " + string.Join(",", features0.Select(x => x.ToString("#.########"))));
        Tensor<float> input0 = new Tensor<float>(new TensorShape(1, features0.Length), features0);
        float[] inputvalues = input0.DownloadToArray();
        print("input: " + string.Join(",", inputvalues.Select(x => x.ToString("#.########"))));
        worker = new Worker(runtimeModel, BackendType.CPU);
        worker.Schedule(input0);
        Tensor<float> output = worker.PeekOutput() as Tensor<float>;

        //print output

        float[] outputValues0 = output.DownloadToArray();
        int predictedIndex0 = ArgMax(outputValues0);
        string predictedLabel0 = strings[predictedIndex0];



        print(string.Join(",", outputValues0.Select(x => x.ToString("0.00000000"))));
        print("Predicted index: " + predictedIndex0);
        print("Predicted string: " + predictedright);

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
