using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Note : MovingObject
{
    public delegate void MouseClickNote([Optional] GameObject _note);
    public static event MouseClickNote clickedOnMouse;
   
    new void Update()
    {
        if (!manager.PlayedNotes.Contains(this)) Destroy(this.gameObject);

        if (this.gameObject == NoteManager.Instance.PlayedNotes.Last() && this.transform.position.x < 0 && !NoteManager.Instance.IsRecording && !NoteManager.Instance.PlayPaused)
        {
            Move();
            NoteManager.Instance.Play();
        }
        Move();
    }

    private void OnMouseDown()
    {
        //clickedOnMouse.Invoke(this.gameObject);
    }

    

}
