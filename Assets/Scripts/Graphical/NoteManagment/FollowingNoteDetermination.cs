using System.Linq;
using UnityEngine;

public class FollowingNoteDetermination : MonoBehaviour
{
    public static FollowingNoteDetermination Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    public Vector3 DeterminNextNote(Vector3[] _notePositions)
    {
        GameObject lastNote = NoteManager.Instance.PlayedNotes.Last();
        Vector3 posOfLastNote = lastNote.transform.position;
        Vector3 mostLiklyNotePosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        foreach (var position in _notePositions)
        {
            float distanceMostLiklyToLast = Vector3.Distance(mostLiklyNotePosition, posOfLastNote);
            float distancePositionToLast = Vector3.Distance(position, posOfLastNote);

            if (distanceMostLiklyToLast > distancePositionToLast)
            {
                mostLiklyNotePosition = position;
            }
        }
        return mostLiklyNotePosition;
    }
}
