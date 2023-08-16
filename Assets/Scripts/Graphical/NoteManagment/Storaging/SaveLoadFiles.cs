using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.WSA;

public class SaveLoadFiles : MonoBehaviour
{
    public TMP_InputField path;
    public void Save()
    {
        string file = $"BPM:{NoteManager.Instance.BPM};";
        foreach (GameObject note in NoteManager.Instance.playedNotes) 
        {
            file += $"{note.transform.position.x},{note.transform.position.y},{note.transform.position.z};";
        }

        if(!Directory.Exists("Recordings"))
        {
            Directory.CreateDirectory("Recordings");
        }
        StreamWriter writer = new StreamWriter($"Recordings/{path.text}.csF");
        writer.Write(file);
        writer.Close();
    }
    public void Load()
    {

    }
}
