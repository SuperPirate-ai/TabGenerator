using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingNoteDetermination : MonoBehaviour
{
    public static FollowingNoteDetermination Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    public string DeterminNextNote(string[] _notePositions)
    {
            foreach (var position in _notePositions)
            { }
        return "";
    }
}
