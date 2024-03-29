import datetime as dt
import cv2
import numpy as np
from fastapi import FastAPI
import asyncio
import json


colors = [
    (255, 0  , 0  ),
    (0  , 255, 0  ),
    (0  , 0  , 255),
    
    (255, 255, 0  ),
    (255  , 0  , 255),
    (0  , 255, 255),
    
    
]

global data
data = {}

WIDTH = 800
HEIGHT = 800




def fix(arr):
    if not isinstance(arr, list):
        arr = [arr, arr]
    if len(arr) == 0:
        arr = [0, 0]
    if len(arr) == 1:
        arr = [arr[0], arr[0]]
    return arr


def get_groupname_from_name(arr_name, scaling_groups):
    for group in scaling_groups:
        if arr_name in group:
            return json.dumps(group)
    return arr_name

def get_scaling(arr_name, scaling_groups, group_scalings):
    groupname = get_groupname_from_name(arr_name, scaling_groups)
    return group_scalings.get(groupname)
    
def eval_scaling(arr):
    arr = fix(arr)
    if max(arr) == min(arr) == 0:
        height_scaling = None
    else:
        height_scaling = (HEIGHT-20) / (max(max(arr), - min(arr)) * 2)
    width_scaling = (WIDTH-20) / len(arr)
    return [width_scaling, height_scaling]

def get_tighter_scaling(a, b):
    if a == None:
        return b
    if b == None:
        return a
    return min(a, b)


def animate():
    global data
    # print(dt.datetime.now().strftime("%H:%M:%S.%f") + " " + str(data["time"]))
    while True:
        if data.get("y_data_s") == None:
            continue
        y_data_s, scaling_groups, time = data["y_data_s"], data["common_scaling_groups"], data["time"]
        canvas = np.zeros((WIDTH, HEIGHT, 3), dtype=np.uint8)
        
        group_scalings = {}
               
        for idx, (arr_name, arr) in enumerate(y_data_s.items()):
            group_name = get_groupname_from_name(arr_name, scaling_groups)
            scaling = eval_scaling(arr)
            if group_name:
                if group_name not in group_scalings:
                    group_scalings[group_name] = scaling
                else:
                    if scaling[0]:
                        group_scalings[group_name][0] = get_tighter_scaling(group_scalings[group_name][0], scaling[0])
                    group_scalings[group_name][1] = get_tighter_scaling(group_scalings[group_name][1], scaling[1])
            else:
                if not scaling[0]:
                    scaling[0] = 1
                group_scalings[json.dumps([arr_name])] = scaling
                    
        for idx, (arr_name, arr) in enumerate(y_data_s.items()):
            arr = fix(arr)
            width_scaling, height_scaling = get_scaling(arr_name, scaling_groups, group_scalings)
            if width_scaling is None:
                width_scaling = 1
            if height_scaling is None:
                height_scaling = 1
            
            color = colors[idx % len(colors)]
            # put text on the canvas based on the name
            cv2.putText(canvas, arr_name, (10, 10 + idx * 20), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 1)
            thickness = (len(list(y_data_s.keys())) - idx)
            for x, y in enumerate(arr):
                if x < len(arr) - 1:
                    cv2.line(canvas, (int(x * width_scaling), int(-arr[x] * height_scaling + HEIGHT/2),),
                                     (int((x + 1) * width_scaling), int(-arr[x + 1] * height_scaling + HEIGHT/2)),
                                     color, thickness)
                cv2.circle(canvas, (int(x * width_scaling), int(-y * height_scaling + HEIGHT/2)), thickness*2, color, -1)
        
        cv2.imshow("plot", canvas)
        cv2.waitKey(1)

app = FastAPI()

@app.post("/plot_data")
async def plot_data(_data: dict):
    global data
    data = _data
    return "ok", 200


def start_cv2():
    while True:
        animate()

if __name__ == "__main__":
    import uvicorn
    import threading
    threading.Thread(target=start_cv2).start()
    print("starting")
    uvicorn.run(app, host="0.0.0.0", port=5001)
    