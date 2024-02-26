using UnityEngine;

public class Scroller : MonoBehaviour
{
    Vector2 mouseStartPos;
    Vector2 mouseEndPos;
    float scrollingValue;
    bool hasScrollingStarted;
    bool hasScrollingFinished;
    bool scrolling = false;
    Vector2 mousePosition;

    private void Update()
    {
        if (scrolling)
        {
            scrollingValue = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - mousePosition.x;
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Scroll();

        }
    }
    private void OnMouseDown()
    {
        scrolling = true;
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseUp()
    {
        scrolling = false;
    }
    private void Scroll()
    {
        foreach (var note in NoteManager.Instance.playedNotes)
        {
            note.transform.position += new Vector3(scrollingValue, 0);
        }
    }
}
