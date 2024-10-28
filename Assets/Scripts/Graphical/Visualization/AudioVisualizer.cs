using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public void Visualize(float _frequency)
    {
        Vector3[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(_frequency);
        Vector3 nextNote = FollowingNoteDetermination.Instance.DetermineNextNote(notePos);
        
        NoteManager.Instance.InstantiateNote(nextNote);
    }
}
