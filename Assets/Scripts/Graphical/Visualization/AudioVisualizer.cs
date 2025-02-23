using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    public void Visualize(float _frequency, float[] _results)
    {
        Vector3[] notePos = NoteToVisualPointsConverter.Instance.GetNotePositions(_frequency);

        Vector3 predictedPos = new();


        Dictionary<int, float> resultsSorted = _results
            .Select((value, index) => new { value, index })
            .ToDictionary(pair => pair.index, pair => pair.value);
        
        resultsSorted = resultsSorted.OrderByDescending(pair => pair.Value)
                                     .ToDictionary(pair => pair.Key, pair => pair.Value);


        //print(string.Join(",", resultsSorted.Select(pair => $"{pair.Key}:{pair.Value}")));


        for (int i = 0; i < resultsSorted.Count; i++)
        {
            for(int j = 0; j < notePos.Length; j++)
            {
                int noteString = (int)Mathf.Abs(notePos[j].y+1);
                if (resultsSorted.ElementAt(i).Key == noteString)
                {
                    predictedPos = notePos[j];
                    break;
                }
            }
        }


        // Vector3 nextNote = FollowingNoteDetermination.Instance.DetermineNextNote(notePos);
        print(predictedPos.y);
        NoteManager.Instance.InstantiateNote(predictedPos);
    }
}
