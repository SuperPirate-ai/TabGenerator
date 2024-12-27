import guitarpro
import sys
import os
from os import path
import json
from fractions import Fraction
import shutil
from random import randrange, choice
from copy import deepcopy



string_fret_offsets = [-24, -19, -14, -9 ,-5, 0]


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
directory = path.join("TrainingData","src","songstodecompose")
print("Directory is ", directory)
print(path.join(os.getcwd(),directory))
for filename in os.listdir(path.join(os.getcwd(),directory)):
    if path.isdir(path.join(directory,filename)):
        continue
    print("Processing ", filename)
    file_name = filename
    file_path = path.abspath(r"TrainingData/src/songstodecompose/" + file_name)
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
            if time_distance_before == 0 or time_distance_after == 0: #when multiple notes are played at the same time
                if len(pattern) < 5:
                    pattern = []
                    continue
                pattern.append([1])
                five_notes_patterns.append(pattern)
                pattern = []
                continue


            if isinstance(note[2], Fraction):
                note[2] = float(note[2])

            if len(pattern) < 5:
                pattern.append(note)
            else:
                pattern.append([1])
                five_notes_patterns.append(pattern)
                pattern = []
                pattern.append(note)
        




    bad_patterns = []
    moderate_bad_patterns = deepcopy(five_notes_patterns[:len(five_notes_patterns) //2])
    extreme_bad_patterns = deepcopy(five_notes_patterns[len(five_notes_patterns) //2:])
    print("moderate bad patterns", len(moderate_bad_patterns), "extreme bad patterns", len(extreme_bad_patterns), "good patterns", len(five_notes_patterns))

    #moderate bad patterns
    for ind, pattern in enumerate(moderate_bad_patterns):
        error_rate =50
        
        for _ in range(error_rate):
            ranNote = randrange(0, 5)
            #print(pattern[ranNote])
            fret = pattern[ranNote][1]
            string = pattern[ranNote][0]

            randomShift = choice([-3, -2, 2, 3])
            bad_string = string + randomShift
            if bad_string < 0 or bad_string > 5:
                continue
            bad_fret = fret + (string_fret_offsets[string-1] - string_fret_offsets[bad_string-1])
            
            if bad_fret < 0 or bad_fret > 24:
                continue

            bad_pattern = deepcopy(pattern)
            bad_pattern[ranNote][1] = bad_fret
            bad_pattern[ranNote][0] = bad_string
            bad_pattern[-1] = [0]
            bad_patterns.append(bad_pattern)
            #print("good pattern", pattern)
            #print("bad pattern", bad_pattern)   
            break
       
    
    #extreme bad patterns
    for ind, pattern in enumerate(extreme_bad_patterns):
        error_rate =500
        last_shift =-1
        bad_pattern = deepcopy(pattern)
        for indx_note,_note in enumerate(pattern):
            if(indx_note == len(pattern)-1):
                continue

            for _ in range(error_rate):
                
                fret = pattern[indx_note][1]
                string = pattern[indx_note][0]
                
                if last_shift ==  1:
                    randomShift = choice([5,4,3])
                elif last_shift == -1:
                    randomShift = choice([-5,-4,-3])
                else:
                    randomShift = choice([5,4,3, -3,-4,-5])

                
                
                bad_string = string + randomShift
                if bad_string < 0 or bad_string > 5:
                    continue
                bad_fret = fret + (string_fret_offsets[string-1] - string_fret_offsets[bad_string-1])
                
                if bad_fret < 0 or bad_fret > 24:
                    continue

                bad_pattern[indx_note][1] = bad_fret
                bad_pattern[indx_note][0] = bad_string
                
                print("good string", string, "good fret", fret)
                print("bad string", bad_string, "bad fret", bad_fret)
                print("____________________________________________________________")
                if(randomShift > 0):
                    last_shift = -1
                else:
                    last_shift = 1  
                break
        bad_pattern[-1] = [0]
        bad_patterns.append(bad_pattern)
        

            

    print("bad patterns", len(bad_patterns))
    print("good patterns", len(five_notes_patterns))


    
    pattern_path = path.join(os.getcwd(),"TrainingData","src","dataFiveNotePatterns",(file_name + ".json"))
    with open(pattern_path, "w",encoding='utf-8') as f:
        f.write(json.dumps({ "five_notes_patterns": five_notes_patterns,"bad_patterns": bad_patterns}))
 
    shutil.move(file_path, path.join(os.getcwd(),"TrainingData","src","songstodecompose","processed" , file_name))


    #[string, fret, start]



#--------------------------------------------------------------

