using Accord.Math;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using SNVector2 = System.Numerics.Vector2;
using CL = CustomLogger;
using URange = UnityEngine.RangeAttribute;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using UnityEngine.Networking;
//using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;

public class AudioAnalyser : MonoBehaviour
{
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;
    [SerializeField] AudioVisualizer visualizer;
    [SerializeField] AudioAnalyser audioAnalyser;
    [URange(1f, 1000f)]
    [SerializeField] public float clearReadoutNoiseGateMultiplier = 250.0f;
    [URange(30f, 500f)]
    [SerializeField] public float clearReadoutMinimumFrequency = 150.0f;
    [URange(1, 30)]
    [SerializeField] public double loudnessExpPower = 2.0;
    [URange(0.01f, 1f)]
    [SerializeField] public float minRelativeLoudnessToOvertoneFrequency = .5f;
    [URange(1f, 30f)]
    [SerializeField] public float maxRelativeLoudnessToOvertoneFrequency = 2.0f;
    public int[] stringFret0NoteIndex = new int[6] { 0, 5, 10, 15, 19, 24 };

    private DateTime templastTime = DateTime.Now;

    static double[] frequencies = { 27.5, 29.135, 30.868, 32.703, 34.648, 36.708, 38.891, 41.203, 43.654, 46.249, 48.999, 51.913,
                                 55.0, 58.27, 61.735, 65.406, 69.296, 73.416, 77.782, 82.407, 87.307, 92.499, 97.999, 103.826,
                                 110.0, 116.541, 123.471, 130.813, 138.591, 146.832, 155.563, 164.814, 174.614, 184.997, 195.998, 207.652,
                                 220.0, 233.082, 246.942, 261.626, 277.183, 293.665, 311.127, 329.628, 349.228, 369.994, 391.995, 415.305,
                                 440.0, 466.164, 493.883, 523.251, 554.365, 587.33, 622.254, 659.255, 698.456, 739.989, 783.991, 830.609,
                                 880.0, 932.328, 987.767, 1046.502, 1108.731, 1174.659, 1244.508, 1318.51, 1396.913, 1479.978, 1567.982, 1661.219,
                                 1760.0, 1864.655, 1975.533, 2093.005, 2217.461, 2349.318, 2489.016, 2637.021, 2793.826, 2959.955, 3135.963, 3322.438,
                                 3520.0, 3729.31, 3951.066, 4186.009};

    // Define the note names
    static string[] noteNames = { "A0", "A#0/Bb0", "B0", "C1", "C#1/Db1", "D1", "D#1/Eb1", "E1", "F1", "F#1/Gb1", "G1", "G#1/Ab1",
                               "A1", "A#1/Bb1", "B1", "C2", "C#2/Db2", "D2", "D#2/Eb2", "E2", "F2", "F#2/Gb2", "G2", "G#2/Ab2",
                               "A2", "A#2/Bb2", "B2", "C3", "C#3/Db3", "D3", "D#3/Eb3", "E3", "F3", "F#3/Gb3", "G3", "G#3/Ab3",
                               "A3", "A#3/Bb3", "B3", "C4", "C#4/Db4", "D4", "D#4/Eb4", "E4", "F4", "F#4/Gb4", "G4", "G#4/Ab4",
                               "A4", "A#4/Bb4", "B4", "C5", "C#5/Db5", "D5", "D#5/Eb5", "E5", "F5", "F#5/Gb5", "G5", "G#5/Ab5",
                               "A5", "A#5/Bb5", "B5", "C6", "C#6/Db6", "D6", "D#6/Eb6", "E6", "F6", "F#6/Gb6", "G6", "G#6/Ab6",
                               "A6", "A#6/Bb6", "B6", "C7", "C#7/Db7", "D7", "D#7/Eb7", "E7", "F7", "F#7/Gb7", "G7", "G#7/Ab7",
                               "A7", "A#7/Bb7", "B7", "C8"};

    private int sampleRate;
    private DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    static string GetNoteName(double frequency)
    {
        // Find the note corresponding to the frequency
        int index = Array.BinarySearch(frequencies, frequency);
        if (index < 0)
        {
            // Interpolate the closest notes
            index = ~index;
            if (index == frequencies.Length)
            {
                index--;
            }
            if (index == 0)
            {
                index++;
            }
            int lowerIndex = index - 1;
            double lowerFreq = frequencies[lowerIndex];
            double upperFreq = frequencies[index];
            string lowerNote = noteNames[lowerIndex];
            string upperNote = noteNames[index];

            // Choose the closest note based on frequency
            if (Math.Abs(lowerFreq - frequency) < Math.Abs(upperFreq - frequency))
            {
                return lowerNote;
            }
            else
            {
                return upperNote;
            }
        }
        else
        {
            // Exact match
            return noteNames[index];
        }
    }

    static int GetNoteIndex(string noteName)
    {
        int index = Array.IndexOf(noteNames, noteName);
        if (index < 0)
        {
            throw new ArgumentException("Note not found.");
        }
        return index;
    }

    static double GetNoteFrequency(string noteName)
    {
        int index = Array.IndexOf(noteNames, noteName);
        if (index < 0)
        {
            CL.Log(noteName);
            CL.Log(noteNames);
            CL.Flush();
            throw new ArgumentException("Note not found.");
        }

        return frequencies[index];
    }


    public float[] derivative(float[] orig_data)
    {
        float[] deriv = new float[orig_data.Length - 1];
        for (int i = 0; i < orig_data.Length - 1; i++)
        {
            deriv[i] = orig_data[i + 1] - orig_data[i];
        }
        return deriv;
    }

    private void Awake()
    {
        clearReadoutNoiseGateMultiplier = 70f;
        clearReadoutMinimumFrequency = 150f;
        loudnessExpPower = 7.86;
        minRelativeLoudnessToOvertoneFrequency = .62f;
        maxRelativeLoudnessToOvertoneFrequency = 2f;
    }

    public void Analyse(float[] _rawSamples)
    {
        sampleRate = NoteManager.Instance.DefaultSamplerate;
        getPlayedNotesAndStringInfo(_rawSamples);
    }

    private float normalizeFrequency(float frequency)
    {
        while (frequency < 440.0f || frequency > 880.0f)
        {
            if (frequency < 440.0f) frequency *= 2.0f;
            if (frequency > 880.0f) frequency /= 2.0f;
        }
        return frequency;
    }
    static float GetClosestPowerOfTwo(float value)
    {
        if (value >= 1)
        {
            double logValue = Math.Log(value, 2); // Calculate the logarithm base 2
            float lowerPowerOfTwo = (float)Math.Pow(2, Math.Floor(logValue)); // Get the lower power of two
            float upperPowerOfTwo = (float)Math.Pow(2, Math.Ceiling(logValue)); // Get the upper power of two

            // Determine the closest power of two
            if (value - lowerPowerOfTwo < upperPowerOfTwo - value)
            {
                return lowerPowerOfTwo;
            }
            else
            {
                return upperPowerOfTwo;
            }
        }
        else
        {
            double logValue = Math.Log(1 / Math.Abs(value), 2); // Calculate the logarithm base 2 of the reciprocal
            float lowerPowerOfTwo = 1 / (float)Math.Pow(2, Math.Floor(logValue)); // Get the lower power of two
            float upperPowerOfTwo = 1 / (float)Math.Pow(2, Math.Ceiling(logValue)); // Get the upper power of two

            // Determine the closest power of two
            if (Math.Abs(value - lowerPowerOfTwo) < Math.Abs(upperPowerOfTwo - value))
            {
                return lowerPowerOfTwo;
            }
            else
            {
                return upperPowerOfTwo;
            }
        }
    }

    private List<Tuple<int, string, int>> getPlayedNotesAndStringInfo(float[] _sample)
    {
        AudioComponents.Instance.ListenForEarlyReturn(); // DO NOT FUCKING REMOVE THIS
        List<Tuple<int, string, float, float>> formatNotesBackToOriginalLength(List<Tuple<int, string, float, float>> lst, List<Tuple<int, string, float, float>> possibleNotes)
        {
            List<Tuple<int, string, float, float>> ret = new List<Tuple<int, string, float, float>>();
            for (int i = 0; i < possibleNotes.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < lst.Count; j++)
                {
                    if (lst[j].Item4 == possibleNotes[i].Item4)
                    {
                        ret.Add(lst[j]);
                        found = true;
                        break;
                    }
                }
                if (!found) ret.Add(new Tuple<int, string, float, float>(-1, null, 0, possibleNotes[i].Item4));
            }

            return ret;
        }

        float[] samples = _sample;

        var fft = AudioComponents.Instance.FFT(samples);
        float averageLevel = 0;
        List<Tuple<int, string, float, float>> possibleNotes = new List<Tuple<int, string, float, float>>();

        for (int i = 0; i < fft.Length; i++)
        {
            averageLevel += fft[i];
            float freq = i / (float)fft.Length * (float)sampleRate;
            if (freq > frequencies.Last()*2) { break; } // killing high frequencies
            if (freq < frequencies[0]) { continue; }
            possibleNotes.Add(new Tuple<int, string, float, float>(-1, // note index
                                                                   null, // note name
                                                                   Math.Abs(fft[i]), // level
                                                                   freq)); // frequency
        }
        averageLevel /= fft.Length;
        float clearReadoutNoiseGate = averageLevel * clearReadoutNoiseGateMultiplier;
        ; // the minimum level required to confidently consider a frequency as a note

        List<Tuple<int, string, float, float>> clearReadoutNotes = possibleNotes.Where((a) => a.Item3 > clearReadoutNoiseGate && a.Item4 > clearReadoutMinimumFrequency).ToList();

        if (clearReadoutNotes.Count == 0) return new List<Tuple<int, string, int>>();

        Dictionary<float, double> frequencyScores = new Dictionary<float, double>();
        float maxLevel = -1;
        for (int i = 0; i < clearReadoutNotes.Count; i++)
        {
            float frequency = normalizeFrequency(clearReadoutNotes[i].Item4);
            double loudness = (double)clearReadoutNotes[i].Item3;
            if (loudness > maxLevel) maxLevel = (float)loudness;
            if (frequencyScores.ContainsKey(frequency))
            {
                frequencyScores[frequency] += Math.Pow(loudness, loudnessExpPower);
            }
            else
            {
                frequencyScores.Add(frequency, Math.Pow(loudness, loudnessExpPower));
            }
        }


        if (frequencyScores.Count == 0) return new List<Tuple<int, string, int>>();

        frequencyScores = frequencyScores.OrderByDescending((a) => a.Value).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        Dictionary<string, double> nameScores = new Dictionary<string, double>();
        for (int i = 0; i < clearReadoutNotes.Count; i++)
        {
            float frequency = normalizeFrequency(clearReadoutNotes[i].Item4);
            double loudness = (double)clearReadoutNotes[i].Item3;
            
            string noteName = GetNoteName(frequency);
            if (nameScores.ContainsKey(noteName))
            {
                nameScores[noteName] += loudness;
            }
            else
            {
                nameScores.Add(noteName, loudness);
            }
        }
        nameScores = nameScores.OrderByDescending((a) => a.Value).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        string normalizedOvertoneFrequencyName = nameScores.Keys.ToList().First();

        float loudestOvertoneFrequency = -1;
        float loudestOvertoneFrequencyLevel = -1;
        List<Tuple<int, string, float, float>> allOvertoneNotes = new List<Tuple<int, string, float, float>>();
        for (int i=0; i<possibleNotes.Count; i++)
        {
            if (GetNoteName(normalizeFrequency(possibleNotes[i].Item4)) == normalizedOvertoneFrequencyName) // get if its an overtone frequency
            {
                if (possibleNotes[i].Item3 > loudestOvertoneFrequencyLevel)
                {
                    loudestOvertoneFrequencyLevel = possibleNotes[i].Item3;
                    loudestOvertoneFrequency = possibleNotes[i].Item4;
                }
                allOvertoneNotes.Add(possibleNotes[i]);
            }
        }

                
        float actualFrequency = -1;
        Tuple<int, string, float, float> actualFrequencyNote = new Tuple<int, string, float, float>(
            -1, // note index
                                                                   null, // note name
                                                                   0, // level
                                                                   (float)frequencies[8]);
        float minLoudnessForRealFrequency = loudestOvertoneFrequencyLevel * minRelativeLoudnessToOvertoneFrequency;
        float maxLoudnessForRealFrequency = loudestOvertoneFrequencyLevel * maxRelativeLoudnessToOvertoneFrequency;

        for (int i = 0; i < possibleNotes.Count-1; i++)
        {
            if (possibleNotes[i].Item4 < 80f) continue; // filter for low frequencies, remove for 7 string
            if (!(possibleNotes[i].Item3 > minLoudnessForRealFrequency)) continue;
            if (!(possibleNotes[i].Item3 < maxLoudnessForRealFrequency)) continue;
            float factor = possibleNotes[i].Item4 / loudestOvertoneFrequency;
            actualFrequency = GetClosestPowerOfTwo(factor) * loudestOvertoneFrequency;
            actualFrequencyNote = possibleNotes[i];
            break;
        }

        if (actualFrequency == -1) return new List<Tuple<int, string, int>>();

        string actualNoteName = GetNoteName(actualFrequency);
        int actualNoteIndex = GetNoteIndex(actualNoteName);

        if (!AudioComponents.Instance.DetectPickStroke(maxLevel, actualNoteIndex))
        {
            CL.Clear();
            return new List<Tuple<int, string, int>>();
        }
        DateTime currentTime = DateTime.Now;
        //var vis = new Dictionary<string, object>
        //    {
        //        { "time" , currentTime.ToString("HH:mm:ss")},
        //        { "common_scaling_groups", new string[][] {
        //                new string [] { "possibleNotes" }
        //            }
        //        },
        //        { "y_data_s", new Dictionary <string, object> {
        //                //{ "clearReadoutNoiseGate", clearReadoutNoiseGate },
        //                //{ "minLoudnessForRealFrequency", minLoudnessForRealFrequency },
        //                //{ "maxLoudnessForRealFrequency", maxLoudnessForRealFrequency },
        //                //{ "clearReadoutNotes", formatNotesBackToOriginalLength(clearReadoutNotes, possibleNotes).Select((a) => a.Item3).Take(150).ToList().ToArray()},
        //                { "possibleNotes", possibleNotes.Select((a) => a.Item3).ToList().ToArray()},
        //                //{ "loudestOvertoneFrequency", formatNotesBackToOriginalLength(new List<Tuple<int, string, float, float>>() { new Tuple<int, string, float, float>(-1, null, loudestOvertoneFrequencyLevel, loudestOvertoneFrequency) }, possibleNotes).Select((a) => a.Item3).Take(150).ToList().ToArray()},
        //                //{ "possibleNotes", possibleNotes.Select((a) => a.Item3).Take(150).ToList().ToArray()},
        //            }
        //        }
        //    };
        //audio_visualization_interface.Instance.CallVisualisation(vis);


        CL.Clear();
        CL.Log(actualNoteName);
        var recognizedNote = new Tuple<int, string, int>(actualNoteIndex - 19, actualNoteName, 4);

        int idealFret = -1;
        int idealGtrString = -1;

        if (MicrophoneInput.Instance.calibrating)
        {
            if (!(templastTime.AddSeconds(.2) < DateTime.Now && actualFrequency > 80)) return new List<Tuple<int, string, int>>();
            int minFretClipping = FretboardCalibrationRangeScript.Instance.min;
            int maxFretClipping = FretboardCalibrationRangeScript.Instance.max;
            for (int gtrString = 0; gtrString < stringFret0NoteIndex.Length; gtrString++)
            {
                int testfret = recognizedNote.Item1 - stringFret0NoteIndex[gtrString];
                if (minFretClipping <= testfret && testfret <= maxFretClipping)
                {
                    idealFret = testfret;
                    idealGtrString = gtrString;
                }
            }
            var training_data = new Dictionary<string, object>
            {
                { "currentfret", idealFret},
                { "currentstring", idealGtrString },
                { "notename", actualNoteName },

                { "frequency", actualFrequency },
                { "maxlevel", maxLevel },

                { "overtone1level", 0f },
                { "overtone2level", 0f },
                { "overtone3level", 0f },
                { "overtone4level", 0f },
                { "overtone5level", 0f },
            };

            foreach (var note in allOvertoneNotes)
            {
                int currentnoteindex = GetNoteIndex(GetNoteName(note.Item4));
                int overtoneindex = (currentnoteindex - actualNoteIndex) / 12;
                if (overtoneindex > 5) continue;
                if (overtoneindex < 1) continue;
                training_data[$"overtone{overtoneindex}level"] = note.Item3 + (float)(training_data[$"overtone{overtoneindex}level"]);
            }
            //var vis = new Dictionary<string, object>
            //{
            //    { "time" , currentTime.ToString("HH:mm:ss")},
            //    { "common_scaling_groups", new string[][] {
            //            new string [] { "maxlevel" , "overtonelevel", "fft_arr" }
            //        }
            //    },
            //    { "y_data_s", training_data }

            //};
            //audio_visualization_interface.Instance.CallVisualisation(vis);


            //audio_visualization_interface.Instance.SaveAITrainingData(training_data);
            CL.Log(training_data);
            templastTime = DateTime.Now;
        }

        CL.Flush();



        var testing_data = new Dictionary<string, object>
            {
                { "frequency", actualFrequency },
                { "maxlevel", maxLevel },

                { "overtone1level", 0f },
                { "overtone2level", 0f },
                { "overtone3level", 0f },
                { "overtone4level", 0f },
                { "overtone5level", 0f },
            };

        int overtonenotecounter = 0;
        foreach (var note in allOvertoneNotes)
        {
            int currentnoteindex = GetNoteIndex(GetNoteName(note.Item4));
            int overtoneindex = (currentnoteindex - actualNoteIndex) / 12;
            if (overtoneindex > 5) continue;
            if (overtoneindex < 1) continue;
            testing_data[$"overtone{overtoneindex}level"] = note.Item3 + (float)(testing_data[$"overtone{overtoneindex}level"]);
        }

        IEnumerator invokeTestAITrainingData(Dictionary<string, object> testing_data, Note noteGameObject)
        {

            var json = JsonConvert.SerializeObject(testing_data);
            byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(json);
            UnityWebRequest webRequest = UnityWebRequest.PostWwwForm("http://localhost:5002/predict", "POST");

            // Set the upload handler to upload JSON data
            webRequest.uploadHandler = new UploadHandlerRaw(jsonData);

            // Set the content type to application/json
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text.Trim('"');
                int AI_fret = int.Parse(response);
                int maxFretDistance = int.MaxValue;
                int idealFret = -1;
                int idealGtrString = -1;
                for (int gtrString = 0; gtrString < stringFret0NoteIndex.Length; gtrString++)
                {
                    int testfret = recognizedNote.Item1 - stringFret0NoteIndex[gtrString];
                    if (Math.Abs(testfret - AI_fret) < maxFretDistance)
                    {
                        maxFretDistance = Math.Abs(testfret - AI_fret);
                        idealFret = testfret;
                        idealGtrString = gtrString;
                    }
                }
                NoteManager.Instance.UpdatePositionOnFretboard(noteGameObject, new UnityEngine.Vector2(idealGtrString - 6, idealFret));
            }
        }
       

        try
        {
            int gtr_string = -1;
            int fret = -1;
            if (MicrophoneInput.Instance.calibrating)
            {
                gtr_string = idealGtrString;
                fret = idealFret;
            } else
            {
                gtr_string = recognizedNote.Item3;
                fret = recognizedNote.Item1 - stringFret0NoteIndex[gtr_string];
            }

            if (gtr_string == -1 || fret == -1) return new List<Tuple<int, string, int>>();
            Note noteGameObject = NoteManager.Instance.InstantiateNote(new UnityEngine.Vector3(0, gtr_string - 6, fret));
            if (!MicrophoneInput.Instance.calibrating)
            {
                StartCoroutine(invokeTestAITrainingData(testing_data, noteGameObject));
            }
        }
        catch (Exception e)
        {
            print("Error: " + e.Message);
        }

        return new List<Tuple<int, string, int>>() {  };
    }
}