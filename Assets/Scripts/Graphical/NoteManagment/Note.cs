using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Note : MovingObject
{
    public delegate void MouseClickNote([Optional] GameObject _note);
    public static event MouseClickNote clickedOnMouse;

    new void Update()
    {
        if (!manager.PlayedNotes.Contains(this.gameObject)) Destroy(this.gameObject);

        
    }


    private void OnMouseDown()
    {
        //clickedOnMouse.Invoke(this.gameObject);
    }

}
