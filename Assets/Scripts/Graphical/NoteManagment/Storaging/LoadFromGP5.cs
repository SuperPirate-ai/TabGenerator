using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

    [SerializeField] APIRequest apiRequest;
    GTPFileContent content;

    private void Start()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    private void OnApplicationQuit()
    {
        var result = apiRequest.SendGetRequest(apiAddr, shutdownFileExtension);
    }

    private GTPFileContent ReadGuitarNotesFromAPI(string _filepath)
    {
        print("ReadGuitarNotesFromAPI");
        try
        {
            string jsonResponse = apiRequest.SendGetRequest(apiAddr, readFileExtension + _filepath).Result;
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
        foreach (GameObject note in NoteManager.Instance.PlayedNotes)
        {
            Vector3 pos = note.transform.position;
            notes[i, 0] = pos.x;
            notes[i, 1] = -pos.y;
            notes[i, 2] = pos.z;
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