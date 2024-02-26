//using Unity.Plastic.Newtonsoft.Json;
//using Unity.Plastic.Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
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
    string writeFileExtension = "/writefile";
    
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
    async Task WriteToApi(string _filepath)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            float[,] notes = new float[NoteManager.Instance.playedNotes.Count, 3];
           
            int i = 0;
            foreach(GameObject note in NoteManager.Instance.playedNotes)
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

            var response = await httpClient.PostAsync(apiPath + writeFileExtension, httpContent);

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