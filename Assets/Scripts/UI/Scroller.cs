using UnityEngine;

public class Scroller : MonoBehaviour
{
    Vector2 mouseStartPos;
    Vector2 mouseEndPos;
    float scrollingValue;
    bool hasScrollingStarted;
    bool hasScrollingFinished;

    private void Update()
    {
        hasScrollingStarted = Input.GetMouseButtonDown(0);

        if (hasScrollingStarted)
            mouseStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        hasScrollingFinished = Input.GetMouseButtonUp(0) && mouseStartPos != null;

        if (Input.GetMouseButtonUp(0) && mouseStartPos != null)
            Scroll();
    }
    private void Scroll()
    {
        mouseEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        scrollingValue = mouseEndPos.x - mouseStartPos.x;
        foreach (var note in NoteManager.Instance.playedNotes)
        {
            note.transform.position += new Vector3(scrollingValue, 0);
        }
    }
}
