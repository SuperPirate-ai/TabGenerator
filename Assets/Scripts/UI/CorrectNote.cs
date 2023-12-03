using System.Linq;
using UnityEngine;

public class CorrectNote : MonoBehaviour
{
    NoteManager noteManager;
    private void Awake()
    {
        noteManager = NoteManager.Instance;
        Note.clickedOnMouse += ShowAllPosibleNotes;
    }

    public void ShowAllPosibleNotes(GameObject _note)
    {
        Vector3 notePosition = _note.transform.position;
        Vector3[] allReferenzedNotePositions = NoteToVisualPointsConverter.Instance.GetNotePositions(notePosition);
        allReferenzedNotePositions.ToList().ForEach(pos => pos = SetXValuesToActualNotePosition(pos, _note.transform.position.x));

        NoteManager.Instance.InstantiateNotes(allReferenzedNotePositions);
    }
    Vector3 SetXValuesToActualNotePosition(Vector3 _pos, float _actualNoteX)
    {
        return new Vector3(_actualNoteX, _pos.y, _pos.z);
    }


}

