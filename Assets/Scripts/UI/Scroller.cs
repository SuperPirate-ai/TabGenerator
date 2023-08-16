using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroller : MonoBehaviour
{
    Vector2 mouseStartPos;
    Vector2 mouseEndPos;
    float scrollingValue;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            mouseStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        }
        if(Input.GetMouseButtonUp(0) && mouseStartPos != null)
        {
            mouseEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            scrollingValue = mouseEndPos.x - mouseStartPos.x;
            foreach(var note in NoteManager.Instance.playedNotes)
            {
                note.transform.position += new Vector3(scrollingValue, 0);
            }
        }
    }
}
