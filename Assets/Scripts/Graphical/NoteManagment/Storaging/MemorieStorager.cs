using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MemorieStorager : MonoBehaviour
{
    
    public static void StorageNoteInMemory(GameObject note)
    {

        Vector3 pos = note.transform.position;
        pos.z = System.Convert.ToInt16(note.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text);
        NoteManager.Instance.playedNotes.Add(pos);
        Destroy(note.gameObject);
    }
    public static void SaveAllNotes()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
        foreach (var item in notes)
        {
            StorageNoteInMemory(item);

        }
    }
    
    public void LoadFromMemory()
    {
        SaveAllNotes();
        foreach (var notepos in NoteManager.Instance.playedNotes)
        {
            GameObject go = Instantiate(NoteManager.Instance.noteObj, notepos, Quaternion.identity);
            go.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = notepos.z.ToString();
        }
    }


}
