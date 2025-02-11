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
    public Vector3 DetermineNextNote(Vector3[] _notePositions)
    {

        if (_notePositions == null) return new Vector3();
        if (NoteManager.Instance.PlayedNotes.Count == 0)
        {
            float smallestZ = float.MaxValue;
            Vector3 smallestTabNote = new Vector3();
            foreach (Vector3 position in _notePositions)
            {
                if (position.z < smallestZ)
                {
                    smallestZ = position.z;
                    smallestTabNote = position;
                }
            }
            return smallestTabNote;
        }
        Vector3 mostLikelyNotePosition = new Vector3(0, 0, 200);


        GameObject lastNote = NoteManager.Instance.PlayedNotes.Last();
        Vector3 posOfLastNote = lastNote.transform.position;
        foreach (var position in _notePositions)
        {
            float distanceMostLikelyToLast = Mathf.Abs(mostLikelyNotePosition.z - posOfLastNote.z);
            float distancePositionToLast = Mathf.Abs(position.z - posOfLastNote.z);

            //print(distanceMostLikelyToLast + "||" + distancePositionToLast + ": Fret:" + position.z);
            if (distanceMostLikelyToLast > distancePositionToLast)
            {
                mostLikelyNotePosition = position;
            }
        }
        return mostLikelyNotePosition;

    }
}
