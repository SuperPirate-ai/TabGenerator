using System.Collections.Generic;
using System;
using UnityEngine;


public class NoteToVisualPointsConverter : MonoBehaviour
{
    public static NoteToVisualPointsConverter Instance { get; private set; }

    [SerializeField] public NotesSO notes;
    //[SerializeField] public StringReferenzSO guitarStringRefernez;
    public int[] stringFret0NoteIndex = new int[6] { 0, 5, 10, 15, 19, 24 };


    private void OnEnable()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    public Vector3[] GetNotePositions(List<Tuple<int, string, int>> recognizedNotes)
    {
        try
        {
            if (recognizedNotes.Count == 0) return new Vector3[] { };
            int gtr_string = recognizedNotes[0].Item3;
            int fret = recognizedNotes[0].Item1 - stringFret0NoteIndex[gtr_string];

            if (fret < 0) return new Vector3[] { };
            return new Vector3[] { new Vector3(0, gtr_string-6, fret) };
        } catch (Exception e) { 
            print("Error: " + e.Message);
            return new Vector3[] { };
        }
    }
    public Vector3[] GetNotePositions(Vector3 _notepos)
    {
        string positionsString = null;
       
        if (positionsString == null) { Debug.LogError("NO POSITIONS FOUND FOR GIVEN POINT!"); return null; }

        string[] posArray = positionsString.Split(";");
        Vector3[] notePositions = new Vector3[posArray.Length];

        for (int i = 0; i < notePositions.Length; i++)
        {
            float y = float.Parse(posArray[i].Split(',')[0]);
            float z = float.Parse(posArray[i].Split(',')[1]);
            notePositions[i] = new Vector3(0, y, z);
        }
        return notePositions;

    }




}