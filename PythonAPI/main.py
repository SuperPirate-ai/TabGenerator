import multiprocessing
import sys
from io import StringIO
import fastapi
from fastapi import FastAPI
import uvicorn
import os
import signal
import guitarpro 
from pathlib import Path as P_Path
from guitarpro import Song

print(sys.stdout)
if sys.stdout is None:
    sys.stdout = StringIO()

app = FastAPI()

should_shut_down = False

@app.get("/readfile")
async def readfile(path: str):
    song = guitarpro.parse(path)

    bpm = str(song.tempo)
    info = bpm
    i = 0
    for track in song.tracks:
        #print(track.header.tempo)
        for measure in track.measures:
            for voice in measure.voices:
                for beat in voice.beats:
                    for note in beat.notes:
                        
                        note_startpoint = str((note.beat.start-960)/100)
                        note_value = str(note.value)
                        note_string = str(-note.string)

                        info += ";" + note_string + "," + note_value + "," +  note_startpoint

    return {"message": info}

@app.post("/writefile")
async def writeFile(request:fastapi.Request, path: str = r"C:\Users\peer\Desktop\UnityBuilds\ThisISCool.gp5",):# add atributes as parameter

    info = dict(await request.json())
    notes = info["notes"]
    p = P_Path(path)
    s = Song()
    s.title = p.stem
    t = guitarpro.Track(s)
    s.tracks = [t]
    t.measures = []

    curr_time = None

    for note in notes:
        if curr_time == note[0]:
            continue
        curr_time = note[0]
        add_new_measure(t,int(note[2]), int(note[1]))




    guitarpro.write(s, path)


@app.get("/shutdown")
async def stopProgram():
    should_shut_down = True
    os.kill(os.getpid(), signal.SIGTERM)




def add_new_measure(t:guitarpro.Track,fret,string):
    h = guitarpro.models.MeasureHeader()
    m = guitarpro.Measure(t, h)
    v = m.voices[0]
    t.measures.append(m)
    b = guitarpro.models.Beat(v)
    # b.start = 960
    n = guitarpro.models.Note(b, fret, 95, string)
    b.notes = [n]
    v.beats.append(b)
    t.song.addMeasureHeader(h)
    
if __name__ == "__main__":
    multiprocessing.freeze_support()
    uvicorn.run(app, host="0.0.0.0", port=5000, log_level="info")


