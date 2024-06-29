using System.Diagnostics;
using UnityEngine;

public class StartAPI : MonoBehaviour
{
    private string nativePythonAPIPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "\\PythonAPI\\main.py";
    private string compiledAPIPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "\\PythonAPI\\dist\\BACKENDAPI.exe";

    private void Awake()
    {
        Process process = new Process();
        string arguments = "";

        if (System.IO.File.Exists(compiledAPIPath))
        {
            if (Application.isEditor)
            {
                UnityEngine.Debug.LogWarning("API is compiled, but you are running in editor.");
            }
            process.StartInfo.FileName = compiledAPIPath;
        }
        else
        {
            process.StartInfo.FileName = "cmd.exe";
            arguments = "/C python.exe " + nativePythonAPIPath;
        }

        process.StartInfo.Arguments = arguments;
        process.Start();
    }


}
