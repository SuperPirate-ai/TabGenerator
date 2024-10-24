using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class NoteToVisualPointsConverter : MonoBehaviour
{
    public static NoteToVisualPointsConverter Instance { get; private set; }

    [SerializeField] NotesSO notes;
    [SerializeField] StringReferenzSO guitarStringRefernez;
    [SerializeField] Transform pointerTransform;

    //Debuging UI
    [SerializeField] Slider NotePosSlider;
    private void OnEnable()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    public Vector3[] GetNotePositions(float _frequency)
    {
        int indexOfNote = -1;

        for (int i = 0; i < notes.frequnecys.Length; i++)
        {
            if (notes.frequnecys[i] == _frequency)
            {
                indexOfNote = i;
                break;
            }
        }

        if (indexOfNote == -1) { Debug.Log($"No Note found with the Frequency {_frequency}."); return null; }

        string[] notePositionsString = guitarStringRefernez.NotePositions[indexOfNote].Split(';');
        
        int notePosCount = 0;
        for (int i = 0; i < notePositionsString.Length; i++)
        {
            if (InRangeOfSlider(i))
            {
                notePosCount++;
            }
        }

        Vector3[] notePositionsVector = new Vector3[notePosCount];
        notePositionsString = notePositionsString.Reverse().ToArray();
        for (int i = 0; i < notePositionsString.Length; i++)
        {
            if(InRangeOfSlider(i))
            { 
                string[] point = notePositionsString[i].Split(',');
                print(i + "||" + point[1]); 
                notePositionsVector[i] = new Vector3(pointerTransform.position.x, System.Convert.ToInt32(point[0]), System.Convert.ToInt32(point[1]));
            }
        }


        return notePositionsVector;
    }
    private bool InRangeOfSlider(int i)
    {
        return i < NotePosSlider.value;
    }
    public Vector3[] GetNotePositions(Vector3 _notepos)
    {
        string positionsString = null;
        foreach (string item in guitarStringRefernez.NotePositions)
        {
            if (item.Contains($"{_notepos.y},{_notepos.z}"))
            {
                positionsString = item;
                break;
            }
        }
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