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
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    public Vector3[] GetNotePositions(string _note)
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

        string[] notePositionsString = guitarStringRefernez.NotePositions[indexOfNote].Split(';');
        Vector3[] notePositionsVector = new Vector3[notePositionsString.Length];

        for (int i = 0; i < notePositionsString.Length; i++)
        {
            string[] point = notePositionsString[i].Split(',');
            notePositionsVector[i] = new Vector3(0, System.Convert.ToInt32(point[0]), System.Convert.ToInt32(point[1]));
        }
        return notePositionsVector;
    }



}