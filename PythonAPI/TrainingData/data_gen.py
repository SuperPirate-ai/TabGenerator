import guitarpro
import sys
import os
import json
from fractions import Fraction
import shutil
from random import randrange

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
        
        if "instrument" in repr(prop).lower() or "instrument" in str(prop).lower():
            print("FOUND                                            :",i,str(prop), repr(prop))
            
        if find_bpm_object_recursive(prop) is None:
            print("Parent is", prop)
    return None
        


#--------------------------------------------------------------
directory = "src/songstodecompose"
for filename in os.listdir(directory):
    if os.path.isdir(os.path.join(directory,filename)):
        continue
    print("Processing ", filename)
    file_name = filename
    file_path = os.path.abspath(r"src/songstodecompose/" + file_name)
    file = guitarpro.parse(file_path, encoding="utf-8")

    # get all the notes in the track
    five_notes_patterns = []
    denominator = None
    for track in file.tracks:

        user_in = input("Do you want to use this track? " + track.name + "|| " + track.rse.instrument.effect + ": ")
        if user_in == "":
            continue
        notes = []
        tempo = track.song.tempo
        print("tempo is ",tempo)
        for measure in track.measures:
            denominator = measure.timeSignature.denominator.value
            #print(measure.header.tempo)
            
            for voice in measure.voices:
                for beat in voice.beats:
                    for note in beat.notes:
                        notes.append([note.string, note.value, beat.start])
        


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
            if isinstance(note[2], Fraction):
                note[2] = float(note[2])

            if len(pattern) < 5:
                pattern.append(note)
            else:
                five_notes_patterns.append(pattern)
                pattern = []
                pattern.append(note)
        



    for ind, pattern in enumerate(five_notes_patterns):
        ranNote = randrange(0, 4)
        if ind %2 == 0:

            if pattern[ranNote][0] > 4:
                pattern[ranNote][0] = pattern[ranNote][0] - 2
            else:
                pattern[ranNote][0] = pattern[ranNote][0] + 2
            
            pattern.append(0)
        else:
            pattern.append(1)
        

    with open("src/dataFiveNotePatterns/" + file_name  + ".json", "w",encoding='utf-8') as f:
        f.write(json.dumps({ "five_notes_patterns": five_notes_patterns}))

    shutil.move(file_path, "src/songstodecompose/processed/" + file_name)



#--------------------------------------------------------------