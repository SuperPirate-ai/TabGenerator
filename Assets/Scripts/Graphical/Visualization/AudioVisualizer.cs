using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public void Visualize(float _frequency)
    {
        Vector3[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(_frequency);
        NoteManager.Instance.InstantiateNotes(notePos);
    }
}
