using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

    string apiAddr = "http://localhost:5000";
    string readFileExtension = "/readfile?path=";
    string shutdownFileExtension = "/shutdown";
    string writeFileExtension = "/writefile?path=";

    private string nativePythonAPIPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "\\PythonAPI\\main.py";
    private string compiledAPIPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "\\PythonAPI\\dist\\BACKENDAPI.exe";
    [HideInInspector] private string Filepath = Environment.CurrentDirectory + "//AudioFiles//the-eagles-hotel_california.gp3";
    

    [SerializeField]APIRequest apiRequest;
    GTPFileContent content;

    private void Awake()
    {
        if(PlayerPrefs.GetInt("API") != 1)
        {
            PlayerPrefs.SetInt("API", 1);
        }
        print(this.gameObject);
        
        print(apiRequest);
    }
    private void Start()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    private void OnApplicationQuit()
    {
        apiRequest.SendGetRequest(apiAddr, shutdownFileExtension);
    }
    
    //void StartAPI()
    //{
    //    print(nativePythonAPIPath);
    //    Process process = new Process();
    //    string arguments = "";
        
    //    if(System.IO.File.Exists(compiledAPIPath))
    //    {
    //        if(Application.isEditor)
    //        {
    //            UnityEngine.Debug.LogWarning("API is compiled, but you are running in editor.");
    //        }
    //        process.StartInfo.FileName = compiledAPIPath;
    //    }
    //    else
    //    {
    //        process.StartInfo.FileName = "cmd.exe";
    //        arguments = "/C python.exe " + nativePythonAPIPath;
    //    }
        
    //    process.StartInfo.Arguments = arguments;
    //    process.Start(); 
    //}
    private GTPFileContent ReadGuitarNotesFromAPI(string _filepath)
    {
        print("ReadGuitarNotesFromAPI");
        try
        { 
            string jsonResponse = apiRequest.SendGetRequest(apiAddr, readFileExtension + _filepath);
            print(jsonResponse);
            GTPFileContent content = JsonConvert.DeserializeObject<GTPFileContent>(jsonResponse);
            return content;
        }
        catch (Exception e)
        {
            print(e);
            return null;
        }
    }   
    private void WriteToApi(string _filepath)
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
        apiRequest.SendPostRequest(content, apiAddr, writeFileExtension, _filepath);
        
       
    }

    public string GetStandard(string _path)
    {
        try
        {
            GTPFileContent fileContent = ReadGuitarNotesFromAPI(_path);

            print($"fileContent{fileContent.message}");
            return fileContent.message;
        }
        catch (Exception e)
        {
            print(e);
            return null;
        }
    }
    public void SendStandard(string _path)
    {
        WriteToApi(_path);
    }
}