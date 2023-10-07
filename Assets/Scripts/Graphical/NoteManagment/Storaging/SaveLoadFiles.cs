using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class SaveLoadFiles : MonoBehaviour
{
    [SerializeField] TMP_InputField fileName;
    [SerializeField] TMP_Text errorMes;
    NoteManager noteManager;
    private readonly string internalPath = "Recordings";

    private void Awake()
    {
        noteManager = NoteManager.Instance;
    }
    private void OnEnable()
    {
        errorMes.gameObject.SetActive(false);
        CreateDirectory(internalPath);
    }
    public void Save()
    {
        string file = $"{noteManager.BPM}";
        foreach (GameObject note in noteManager.playedNotes)
        {
            file += $";{note.transform.position.y},{note.transform.position.z},{note.transform.position.x}";
        }

        StreamWriter writer = new StreamWriter($"{internalPath}/{fileName.text}.csF");
        writer.Write(file);
        writer.Close();
    }
    public void Load()
    {
        if (DoesRecordingExists($"{internalPath}/{fileName.text}") == false)
        {
            ThrowErrorMessage();
            return;
        }

        errorMes.gameObject.SetActive(false);

        string file;
        StreamReader reader = new StreamReader($"{internalPath}/{fileName.text}" + (fileName.text.EndsWith(".csF") ? "" : ".csF"));

        file = reader.ReadToEnd();
        reader.Close();

        noteManager.playedNotes.Clear();

        string[] data = file.Split(";");
        noteManager.BPM = Convert.ToInt32(data[0]);

        Vector3[] notepos = new Vector3[data.Length - 1];
        for (int i = 1; i < data.Length; i++)
        {
            int[] pos =  data[i].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            notepos[i] = new Vector3(pos[2], pos[0], pos[1]);
        }
        noteManager.InstantiateNotes(notepos, false);

    }

    private void CreateDirectory(string _drName)
    {
        if (!Directory.Exists(_drName))
        {
            Directory.CreateDirectory(_drName);
        }
    }
    private bool DoesRecordingExists(string _path)
    {
        bool exists = File.Exists($"{_path}.csF") || File.Exists(_path);
        return exists;
    }
    private void ThrowErrorMessage()
    {
        errorMes.gameObject.SetActive(true);
    }
}
