//using Unity.Plastic.Newtonsoft.Json;
//using Unity.Plastic.Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;


public class GTPFileContent
{
    [JsonProperty]
    public string message;
}

public class LoadFromGP5 : MonoBehaviour
{
    public static LoadFromGP5 Instance;

    string apiPath = "http://localhost:5000";
    string readFileExtension = "/readfile?path=";
    string shutdownFileExtension = "/shutdown";
    public string Filepath = "C://Users//peer//UnityProjects//TabGenerator//Assets//AudioFiles//the-eagles-hotel_california.gp3";

    GTPFileContent content;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
        StartAPI();
    }
    private void OnApplicationQuit()
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
            var httpResponseMessage = httpClient.GetAsync(apiPath + shutdownFileExtension).Result;
        }
        catch { }
    }
    void StartAPI()
    {
        string command = "/C python C:\\Ben\\Programs\\PyAPI_Tabgen\\main.py";
        Process.Start("cmd.exe", command);
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
            var httpResponseMessage = httpClient.GetAsync(apiPath + readFileExtension + _filepath).Result;
            string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            content = JsonConvert.DeserializeObject<GTPFileContent>(jsonResponse);
            print(content.message);
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    public string GetStandard(string _path)
    {
        ReadFromURL(_path);
        return content.message;
    }
}