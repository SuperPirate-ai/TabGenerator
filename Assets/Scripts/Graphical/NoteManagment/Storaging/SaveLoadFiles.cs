using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadFiles : MonoBehaviour
{
    [SerializeField] TMP_InputField fileNameInput;
    [SerializeField] TMP_Text errorMes;
    [SerializeField] Toggle useGTPFileFormatChBox;
    NoteManager noteManager;
    private readonly string internalPath = "Recordings";
    private readonly string internalPathGTP = "GTP_Recordings";


    private void Awake()
    {
        noteManager = NoteManager.Instance;
    }
    private void OnEnable()
    {
        errorMes.gameObject.SetActive(false);
        CreateDirectory(internalPath);
        CreateDirectory(internalPathGTP);
    }
    public void Load()
    {
        print("load");
        if (useGTPFileFormatChBox.isOn)
        {
            string path = internalPathGTP + $"\\" + fileNameInput.text;
            print($"gtp || {path}");
            try
            {
                string file = LoadFromGP5.Instance.GetStandard(path);
            }
            catch (Exception e)
            {
                print(e);
            }
            //DisplayNotes(file);
        }
        //LoadAsStandardFile();
    }
    public void Save()
    {
        if (useGTPFileFormatChBox.isOn)
        {
            string path = internalPathGTP + $"\\" + fileNameInput.text;
            LoadFromGP5.Instance.SendStandard(path);
        }
        SaveAsStandartFile();
    }

    void SaveAsStandartFile()
    {
        string file = $"{noteManager.BPM}";
        foreach (GameObject note in noteManager.PlayedNotes)
        {
            file += $";{note.transform.position.y},{note.transform.position.z},{note.transform.position.x}";
        }

        StreamWriter writer = new StreamWriter($"{internalPath}/{fileNameInput.text}.csF");
        writer.Write(file);
        writer.Close();
    }
    void LoadAsStandardFile()
    {
        if (DoesRecordingExists($"{internalPath}/{fileNameInput.text}") == false)
        {
            ThrowErrorMessage();
            return;
        }

        errorMes.gameObject.SetActive(false);

        string file;
        StreamReader reader = new StreamReader($"{internalPath}/{fileNameInput.text}" + (fileNameInput.text.EndsWith(".csF") ? "" : ".csF"));

        file = reader.ReadToEnd();
        reader.Close();
        DisplayNotes(file);

    }
    void DisplayNotes(string _file)
    {
        noteManager.PlayedNotes.Clear();

        string[] data = _file.Split(";");
        noteManager.BPM = Convert.ToInt32(data[0]);

        Vector3[] notepos = new Vector3[data.Length - 1];
        for (int i = 1; i < data.Length; i++)
        {
            float[] pos = data[i].Split(',').Select(n => float.Parse(n)).ToArray();

            notepos[i - 1] = new Vector3(pos[2], pos[0], pos[1]);
        }
        noteManager.InstantiateNotes(notepos);
    }

    private void CreateDirectory(string _drName)
    {
        if (!Directory.Exists(_drName))
        {
            Directory.CreateDirectory(_drName);
        }
    }
    private bool DoesRecordingExists(string _path)
    {
        bool exists = File.Exists($"{_path}.csF") || File.Exists(_path);
        return exists;
    }
    private void ThrowErrorMessage()
    {
        errorMes.gameObject.SetActive(true);
    }
}