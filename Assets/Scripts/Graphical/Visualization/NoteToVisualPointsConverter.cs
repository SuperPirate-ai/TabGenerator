using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NoteToVisualPointsConverter : MonoBehaviour
{
    public static NoteToVisualPointsConverter Instance { get; private set; }

    [SerializeField] NotesSO notes;
    [SerializeField] StringReferenzSO guitarStringRefernez;


    private void OnEnable()
    {
        if(Instance != null) Destroy(this);
        Instance = this;
    }
    public string[] GetNotePositions(string _note)
    {
        int indexOfNote = -1;
        
        for (int i = 0; i < notes.noteNames.Length; i++)
        {
            if (notes.noteNames[i] == _note)
            {
                indexOfNote = i;
                break;
            }
        }

        if (indexOfNote == -1) { Debug.Log($"No Note found with the name{_note}."); return null; }

        string[] notePositions = guitarStringRefernez.NotePositions[indexOfNote].Split(';');
        return notePositions;
    }

    
    
}
