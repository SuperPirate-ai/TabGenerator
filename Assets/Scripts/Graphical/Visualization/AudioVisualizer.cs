using System.Collections.Generic;
using System;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public void Visualize(List<Tuple<int, string, int>> recognizedNotes)
    {
        Vector3[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(recognizedNotes);
        NoteManager.Instance.InstantiateNotes(notePos);
    }
}
