import guitarpro
import numpy as np
import sys
sys.setrecursionlimit(15000000)

alreadyCalledObejcts = []
def find_bpm_object_recursive(properties):
    try:
        dictx = properties.__dict__
    except:
        return
    for i,prop in properties.__dict__.items():
        if prop in alreadyCalledObejcts:
            continue
        alreadyCalledObejcts.append(prop)
        
        if "200" in repr(prop).lower() or "200" in str(prop).lower():
            print("FOUND                                            :",i,str(prop), repr(prop))
            return prop
        if find_bpm_object_recursive(prop) is not None:
            print("Parent is", prop)
    return None
        

file = guitarpro.parse(r"C://Ben//UnityProjects//TabGenerator//PythonAPI//TrainingData//src//death_symbolic.gp5", encoding="utf-8")

# get all the notes in the track
notes = []
tempo = None
denominator = None
for track in file.tracks:
    if track.name.startswith("Chuck Schuldiner"):
        print("tempo is ", track.song.tempo)
        tempo = track.song.tempo
        for measure in track.measures:
            denominator = measure.timeSignature.denominator.value
            #print(measure.header.tempo)
            
            for voice in measure.voices:
                for beat in voice.beats:
                    for note in beat.notes:
                        notes.append([note.string, note.value, beat.start])
        break


five_notes_patterns = []
pattern = []
for i_n, note in enumerate(notes):
    if i_n == 0:
        continue
    if i_n > len(notes)-2:
        break
    time_distance_before = note[2] - notes[i_n-1][2]
    time_distance_after = notes[i_n+1][2] - note[2]
    if time_distance_before == 0 or time_distance_after == 0:
        if len(pattern) < 5:
            pattern = []
            continue

        five_notes_patterns.append(pattern)
        pattern = []
        continue
    if len(pattern) < 5:
        pattern.append(note)
    else:
        five_notes_patterns.append(pattern)
        pattern = []

for pattern in five_notes_patterns:
    print(pattern)

