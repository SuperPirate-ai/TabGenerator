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
        MemorieStorager.SaveAllNotes();
        string file = $"BPM:{NoteManager.Instance.BPM};";
        foreach (Vector3 pos in NoteManager.Instance.playedNotes) 
        {
            file += $"{pos.x},{pos.y},{pos.z};";
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
