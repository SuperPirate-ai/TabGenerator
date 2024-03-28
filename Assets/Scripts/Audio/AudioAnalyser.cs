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
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;

public class AudioAnalyser : MonoBehaviour
{
    [SerializeField] NotesSO notesSO;
    [SerializeField] int analysingDepth;
    [SerializeField] AudioVisualizer visualizer;

    private int sampleRate;

    private List<float> notesFrequencies;
    private string[] noteNames = new string[12] {
        "E",
        "F",
        "F#",
        "G",
        "G#",
        "A",
        "A#",
        "B",
        "C",
        "C#",
        "D",
        "D#",
    };

    private float fftError;

    public static double QuadraticInterpolation(double x0, double y0,
                                            double x1, double y1,
                                            double x2, double y2,
                                            double x)
    {
        // Calculate the interpolation coefficients
        double c0 = (x - x1) * (x - x2) / ((x0 - x1) * (x0 - x2));
        double c1 = (x - x0) * (x - x2) / ((x1 - x0) * (x1 - x2));
        double c2 = (x - x0) * (x - x1) / ((x2 - x0) * (x2 - x1));

        // Perform the interpolation
        double interpolatedValue = y0 * c0 + y1 * c1 + y2 * c2;

        return interpolatedValue;
    }

    static string GetNoteName(double frequency)
    {
        // Define the frequency ranges for each note
        double[] frequencies = { 27.5, 29.135, 30.868, 32.703, 34.648, 36.708, 38.891, 41.203, 43.654, 46.249, 48.999, 51.913,
                                 55.0, 58.27, 61.735, 65.406, 69.296, 73.416, 77.782, 82.407, 87.307, 92.499, 97.999, 103.826,
                                 110.0, 116.541, 123.471, 130.813, 138.591, 146.832, 155.563, 164.814, 174.614, 184.997, 195.998, 207.652,
                                 220.0, 233.082, 246.942, 261.626, 277.183, 293.665, 311.127, 329.628, 349.228, 369.994, 391.995, 415.305,
                                 440.0, 466.164, 493.883, 523.251, 554.365, 587.33, 622.254, 659.255, 698.456, 739.989, 783.991, 830.609,
                                 880.0, 932.328, 987.767, 1046.502, 1108.731, 1174.659, 1244.508, 1318.51, 1396.913, 1479.978, 1567.982, 1661.219,
                                 1760.0, 1864.655, 1975.533, 2093.005, 2217.461, 2349.318, 2489.016, 2637.021, 2793.826, 2959.955, 3135.963, 3322.438,
                                 3520.0, 3729.31, 3951.066, 4186.009};

        // Define the note names
        string[] noteNames = { "A0", "A#0/Bb0", "B0", "C1", "C#1/Db1", "D1", "D#1/Eb1", "E1", "F1", "F#1/Gb1", "G1", "G#1/Ab1",
                               "A1", "A#1/Bb1", "B1", "C2", "C#2/Db2", "D2", "D#2/Eb2", "E2", "F2", "F#2/Gb2", "G2", "G#2/Ab2",
                               "A2", "A#2/Bb2", "B2", "C3", "C#3/Db3", "D3", "D#3/Eb3", "E3", "F3", "F#3/Gb3", "G3", "G#3/Ab3",
                               "A3", "A#3/Bb3", "B3", "C4", "C#4/Db4", "D4", "D#4/Eb4", "E4", "F4", "F#4/Gb4", "G4", "G#4/Ab4",
                               "A4", "A#4/Bb4", "B4", "C5", "C#5/Db5", "D5", "D#5/Eb5", "E5", "F5", "F#5/Gb5", "G5", "G#5/Ab5",
                               "A5", "A#5/Bb5", "B5", "C6", "C#6/Db6", "D6", "D#6/Eb6", "E6", "F6", "F#6/Gb6", "G6", "G#6/Ab6",
                               "A6", "A#6/Bb6", "B6", "C7", "C#7/Db7", "D7", "D#7/Eb7", "E7", "F7", "F#7/Gb7", "G7", "G#7/Ab7",
                               "A7", "A#7/Bb7", "B7", "C8", "C#8/Db8", "D8", "D#8/Eb8", "E8", "F8", "F#8/Gb8", "G8", "G#8/Ab8",
                               "A8"};

        // Find the note corresponding to the frequency
        int index = Array.BinarySearch(frequencies, frequency);
        if (index < 0)
        {
            // Interpolate the closest notes
            index = ~index;
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


    // write a function that does the interpolation from the function getPlayedNotesAndStringInfo using the QuadraticInterpolation function and returns the note index. The function should only take the frequency as an argument and return the note index.
    public int GetNoteIndex(float _frequency)
    {
        float ref1 = -1;
        float ref2 = -1;
        float ref3 = -1;
        int noteindex = -1;
        for (int j = 0; j < notesFrequencies.Count; j++)
        {
            if (_frequency < notesFrequencies[j])
            {
                int baseline = Math.Clamp(j - 1, 0, notesFrequencies.Count - 3);
                ref1 = notesFrequencies[baseline];
                ref2 = notesFrequencies[baseline + 1];
                ref3 = notesFrequencies[baseline + 2];
                double unroundedIdx = QuadraticInterpolation(ref1, baseline, ref2, baseline + 1, ref3, baseline + 2, _frequency);
                noteindex = (int)Math.Round(unroundedIdx, 0);
                return noteindex;
            }
        }
        return -1;
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

    public int GetNoteIndex_lin(float _frequency)
    {
        var closestValue = notesFrequencies.Select((value, index) => new { Value = value, Index = index, Difference = Math.Abs(value - _frequency) })
            .OrderBy(v => v.Difference)
            .First();
        return closestValue.Index;
    }

    private void Awake()
    {

        notesFrequencies = new List<float>();
        for (int i = 0; i < notesSO.frequencies.Length; i++)
        {
            notesFrequencies.Add(notesSO.frequencies[i]);
        }

        sampleRate = NoteManager.Instance.DefaultSamplerate;
        fftError = sampleRate / NoteManager.Instance.DefaultBufferSize;

    }

    public void Analyse(float[] _rawSamples)
    {
        List<Tuple<int, string, int>> recognizedNotes = getPlayedNotesAndStringInfo(_rawSamples);
        if (recognizedNotes.Count() == 0) return;

        visualizer.Visualize(recognizedNotes);

    }


    private List<Tuple<int, string, int>> getPlayedNotesAndStringInfo(float[] _sample)
    {

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

        CL.Clear();
        float[] samples = _sample;

        var fft = AudioComponents.Instance.FFT(samples);
        float averageLevel = 0;
        List<Tuple<int, string, float, float>> possibleNotes = new List<Tuple<int, string, float, float>>();

        for (int i = 0; i < fft.Length; i++)
        {
            averageLevel += fft[i];
            float freq = i / (float)fft.Length * (float)sampleRate;
            if (freq > 10000) { break; } // killing high frequencies
            possibleNotes.Add(new Tuple<int, string, float, float>(-1, // note index
                                                                   null, // note name
                                                                   Math.Abs(fft[i]), // level
                                                                   freq)); // frequency
        }
        averageLevel /= fft.Length;
        float clearReadoutNoiseGate = averageLevel * 250; // the minimum level required to confidently consider a frequency as a note
        float clearReadoutMinimumFrequency = 150; // the minimum frequency required to confidently consider a frequency as a note
        float octaveIndicatingNoiseGate = averageLevel * 125; // the minimum level required to confidently consider a close note to be the root octave

        List<Tuple<int, string, float, float>> clearReadoutFrequencies = possibleNotes.Where((a) => a.Item3 > clearReadoutNoiseGate && a.Item4 > clearReadoutMinimumFrequency).ToList();

        List<Tuple<int, string, float, float>> octaveIndicatingFrequencies = possibleNotes.Where((a) => a.Item3 > octaveIndicatingNoiseGate).ToList();


        Dictionary<float, double> frequencyScores = new Dictionary<float, double>();
        for (int i = 0; i < clearReadoutFrequencies.Count; i++)
        {
            float frequency = clearReadoutFrequencies[i].Item4;
            double loudness = (double)clearReadoutFrequencies[i].Item3;
            while (frequency < 261 || frequency > 523)
            {
                if (frequency < 261) frequency *= 2;
                if (frequency > 523) frequency /= 2;
            }
            if (frequencyScores.ContainsKey(frequency))
            {
                frequencyScores[frequency] += loudness*loudness;
            }
            else
            {
                frequencyScores.Add(frequency, loudness * loudness);
            }
        }
        if (frequencyScores.Count == 0) return new List<Tuple<int, string, int>>();
        //frequencyScores = frequencyScores.OrderByDescending((a) => a.Value).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
        Dictionary<string, double> nameScores = new Dictionary<string, double>();
        for (int i = 0; i < clearReadoutFrequencies.Count; i++)
        {
            float frequency = clearReadoutFrequencies[i].Item4;
            double loudness = (double)clearReadoutFrequencies[i].Item3;
            while (frequency < 261 || frequency > 523)
            {
                if (frequency < 261) frequency *= 2;
                if (frequency > 523) frequency /= 2;
            }
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
        CL.Log(nameScores.Keys.ToList());




        DateTime currentTime = DateTime.Now;
        var vis = new Dictionary<string, object>
            {
                { "time" , currentTime.ToString("HH:mm:ss")},
                { "common_scaling_groups", new string[][] {
                        new string [] { "minimumLevel", "minimumLevel2", "selectednotes", "selectednotes2" }
                    }
                },
                { "y_data_s", new Dictionary <string, object> {
                        { "minimumLevel", clearReadoutNoiseGate },
                        { "minimumLevel2", octaveIndicatingNoiseGate *.95f },
                        { "selectednotes", formatNotesBackToOriginalLength(clearReadoutFrequencies, possibleNotes).Select((a) => a.Item3).Take(200).ToList().ToArray()},
                        { "selectednotes2", formatNotesBackToOriginalLength(octaveIndicatingFrequencies, possibleNotes).Select((a) => a.Item3*.95f).Take(200).ToList().ToArray()}
                    }
                }
            };
        audio_visualization_interface.Instance.CallVisualisation(vis);

        //fftRealAndIndex.Sort((a, b) => b.Y.CompareTo(a.Y)); // sorting by level, highest first

        //int lowest_maybe_inprecise_note_index = 99999;
        //for (int i = 0; i < fftRealAndIndex.Count; i++)
        //{
        //    if (possibleNotes.Count >= 100) break;
        //    float frequency = fftRealAndIndex[i].Y;
        //    float loudness = fftRealAndIndex[i].X;
        //    int noteindex = GetNoteIndex(frequency);
        //    if (noteindex < 0 || averageLevel * 100> loudness)
        //    {
        //        continue;
        //    }

        //    float corrected_frequency = notesFrequencies[noteindex];
        //    if (lowest_maybe_inprecise_note_index > noteindex) lowest_maybe_inprecise_note_index = noteindex;

        //    possibleNotes.Add(new Tuple<int, string, float, float>(noteindex,
        //                                                           noteNames[noteindex % noteNames.Length],
        //                                                           Math.Abs(loudness),
        //                                                           frequency));
        //}
        //CL.Log(possibleNotes.Count);
        //if (possibleNotes.Count == 0) return new List<Tuple<int, string, int>>();

        //List<List<Tuple<int, string, float, float>>> groupedNotes = possibleNotes.GroupBy(tpl => tpl.Item2)
        //                                                            .Select(grp => grp.ToList())
        //                                                            .ToList(); // grouping frequencies by note
        //CL.Log(groupedNotes);
        //groupedNotes.Sort((a, b) => b.Count.CompareTo(a.Count)); // sorting by count of notes (more often occuring note is likely to be the correct one)
        //List < Tuple<int, string, float, float> > mostFrequentNoteAndOctaves = groupedNotes[0];
        //mostFrequentNoteAndOctaves.Sort((a, b) => b.Item4.CompareTo(a.Item4)); // sorting by frequency
        //for (int i = 0; i < mostFrequentNoteAndOctaves.Count; i++)
        //{
        //    //print($"{mostFrequentNoteAndOctaves[i].Item2} {mostFrequentNoteAndOctaves[i].Item4}");
        //}


        //int actual_octave = lowest_maybe_inprecise_note_index / 12;
        //int actual_noteindex = (mostFrequentNoteAndOctaves[0].Item1 % 12) + 12 * actual_octave;
        //string actual_notename = noteNames[actual_noteindex % noteNames.Length];

        //if (!AudioComponents.Instance.DetectPickStroke(samples, actual_noteindex)) return new List<Tuple<int, string, int>>();



        //possibleNotes.Sort((a, b) => a.Item4.CompareTo(b.Item4));
        //if (possibleNotes.Count > 1)
        //{
        //    float[] noteloudnesses = new float[possibleNotes.Count];
        //    for (int i = 0; i < noteloudnesses.Length; i++)
        //    {
        //        noteloudnesses[i] = possibleNotes[i].Item3;
        //    }
        //    float[] deriv = derivative(noteloudnesses);
        //    int[] bufidxs = new int[noteloudnesses.Length];
        //    DateTime currentTime = DateTime.Now;
        //    var vis = new Dictionary<string, object>
        //    {
        //        { "time" , currentTime.ToString("HH:mm:ss")},
        //        { "y_data_s", new Dictionary <string, float[]> {
        //                { "noteloudnesses", noteloudnesses},
        //                { "derivative", deriv.Concatenate(new float[] { deriv.Last() }) }
        //            }
        //        }
        //    };
        //    audio_visualization_interface.Instance.CallVisualisation(vis);
        //}

        //CL.Log(actual_notename);
        //CL.Log(actual_octave);
        CL.Flush();
        //return new List<Tuple<int, string, int>>() { new Tuple<int, string, int>(actual_noteindex, actual_notename, -6) };
        return new List<Tuple<int, string, int>>();
    }
}