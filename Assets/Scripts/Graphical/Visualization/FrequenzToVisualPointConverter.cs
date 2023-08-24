using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FrequenzToVisualPointConverter : MonoBehaviour
{
    public static FrequenzToVisualPointConverter Instance { get; private set; }

    [SerializeField] NotesSO notes;
    [SerializeField] StringReferenzSO guitarStringRefernez;


    private void OnEnable()
    {
        if(Instance != null) Destroy(this);
        Instance = this;
    }
    public void OnNoteDetected(string _note,bool _isOpenWoundString)
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

        if (indexOfNote == -1) { Debug.Log($"No Note found with the name{_note}."); return; }

        string[] notePositions = guitarStringRefernez.NotePositions[indexOfNote].Split(';');

        
        NoteManager.Instance.InstantiateNotes(notePositions, _isOpenWoundString);
       
    }
    
}
