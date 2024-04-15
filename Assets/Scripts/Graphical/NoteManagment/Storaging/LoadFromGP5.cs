using Codice.Client.Common.GameUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.DedicatedServer;


public class GTPFileContent
{
    [JsonProperty]
    public string message;
}

public class LoadFromGP5 : MonoBehaviour
{
    public static LoadFromGP5 Instance;

    string apiAddr = "http://localhost:5000";
    string readFileExtension = "/readfile?path=";
    string shutdownFileExtension = "/shutdown";
    string writeFileExtension = "/writefile?path=";

    private string nativePythonAPIPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "\\PythonAPI\\main.py";
    private string compiledAPIPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "\\PythonAPI\\dist\\BACKENDAPI.exe";
    [HideInInspector] private string Filepath = Environment.CurrentDirectory + "//AudioFiles//the-eagles-hotel_california.gp3";
    
    GTPFileContent content;

    private void Awake()
    {
        if(PlayerPrefs.GetInt("API") != 1)
        {
            PlayerPrefs.SetInt("API", 1);
        }
        print(this.gameObject);
        StartAPI();

    }
    private void Start()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    private void OnApplicationQuit()
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
            var httpResponseMessage = httpClient.GetAsync(apiAddr + shutdownFileExtension).Result;
        }
        catch { }
    }
    
    void StartAPI()
    {
        print(nativePythonAPIPath);
        Process process = new Process();
        string arguments = "";
        
        if(System.IO.File.Exists(compiledAPIPath))
        {
            if(Application.isEditor)
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

    public void ReadFromAPI()
    {
        ReadFromURL(Filepath);
    }
    async Task ReadFromURL(string _filepath)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
            var httpResponseMessage = httpClient.GetAsync(apiAddr + readFileExtension + _filepath).Result;
            string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            content = JsonConvert.DeserializeObject<GTPFileContent>(jsonResponse);
            print(content.message);
        }
        catch (Exception e)
        {
            print(e);
        }
    }
    async Task WriteToApi(string _filepath)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            float[,] notes = new float[NoteManager.Instance.PlayedNotes.Count, 3];
           
            int i = 0;
            foreach(GameObject note in NoteManager.Instance.PlayedNotes)
            {
                Vector3 pos = note.transform.position;
                notes[i,0] = pos.x;
                notes[i, 1] = -pos.y;
                notes[i,2] = pos.z;
                i++;
            }
            var values = new Dictionary<string, float[,]>
            {
                {"notes", notes }
            };
            var content = JsonConvert.SerializeObject(values);
            var httpContent = new StringContent(content,System.Text.Encoding.UTF8,"application/json");

            var response = await httpClient.PostAsync(apiAddr + writeFileExtension +_filepath, httpContent);

            var responseString = await response.Content.ReadAsStringAsync();
        }
        catch
        {

        }
    }

    public string GetStandard(string _path)
    {
        ReadFromURL(_path);
        return content.message;
    }
    public void SendStandard(string _path)
    {
        WriteToApi(_path);
    }
}