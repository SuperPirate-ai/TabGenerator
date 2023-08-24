using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.WSA;
using Accord.Math;

public class SaveLoadFiles : MonoBehaviour
{
    [SerializeField] TMP_InputField fileName;
    [SerializeField] TMP_Text errorMes;
    NoteManager noteManager;
    string internalPath = "Recordings";
    private void Awake()
    {
        noteManager = NoteManager.Instance;
    }
    private void OnEnable()
    {
        errorMes.gameObject.SetActive(false);
    }
    public void Save()
    {
        string file = $"{noteManager.BPM}";//->bpm
        foreach (GameObject note in noteManager.playedNotes) 
        {
            file += $";{note.transform.position.y},{note.transform.position.z},{note.transform.position.x}";
        }

        if(!Directory.Exists(internalPath))
        {
            Directory.CreateDirectory(internalPath);
        }
        StreamWriter writer = new StreamWriter($"{internalPath}/{fileName.text}.csF");
        writer.Write(file);
        writer.Close();
    }
    public void Load()
    {
        if(!File.Exists($"{internalPath}/{fileName.text}.csF") && !File.Exists($"{internalPath}/{fileName.text}"))
        {
            errorMes.gameObject.SetActive(true);return;
        }
        errorMes.gameObject.SetActive(false);
        string file;
        StreamReader reader = new StreamReader($"{internalPath}/{fileName.text}" + (fileName.text.EndsWith(".csF") ? "" : ".csF"));

        file = reader.ReadToEnd();
        reader.Close();

        noteManager.playedNotes.Clear();
        string[] data = file.Split(";");
        noteManager.BPM = Convert.ToInt16(data[0]);
        string[] notepos = new string[data.Length-1];
        for (int i = 1; i < data.Length; i++)
        {
            notepos[i-1] = data[i];
        }
        noteManager.InstantiateNotes(notepos, false);

    }
}
