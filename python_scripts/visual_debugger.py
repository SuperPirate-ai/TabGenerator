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
    (0  , 255, 255),
    
    (255  , 255, 0  ),
    (255  , 0  , 255),
    
    (255, 0  , 255  ),
    (0  , 255, 255  ),
]

global data
data = {}

WIDTH = 800
HEIGHT = 800


def get_scaling(group_scalings, arr, name):
    width_scaling = None
    height_scaling = None
    for groupname in group_scalings.keys():
        if name in json.loads(groupname):
            width_scaling = group_scalings[groupname][0]
            height_scaling = group_scalings[groupname][1]
            break
    
    if not isinstance(arr, list):
        arr = [arr]
    if len(arr) == 0:
        return None, None
    if len(arr) == 1:
        arr.append(arr[0])
    if width_scaling == None:
        width_scaling = WIDTH / (len(arr) - 1)
    if height_scaling == None:
        if (max(arr)-min(arr)) == 0:
            height_scaling = 1
        else:
            height_scaling = (HEIGHT-20) / (max(max(arr),-min(arr)) * 2)
    return width_scaling, height_scaling


def animate():
    global data
    # print(dt.datetime.now().strftime("%H:%M:%S.%f") + " " + str(data["time"]))
    while True:
        if data.get("y_data_s") == None:
            continue
        y_data_s, scaling_groups, time = data["y_data_s"], data["common_scaling_groups"], data["time"]
        canvas = np.zeros((WIDTH, HEIGHT, 3), dtype=np.uint8)
        group_scalings = {}
        for group in scaling_groups:
            group_name = json.dumps(group)
            group_scalings[group_name] = [None, None]
            for group_member_name in group:
                
                group_member = y_data_s.get(group_member_name)
                if not group_member:
                    continue
                if not isinstance(group_member, list):
                    group_member = [group_member]
                if len(group_member) == 0:
                    continue
                if len(group_member) == 1:
                    group_member.append(group_member[0])
                width_scaling = WIDTH / (len(group_member) - 1)
                if max(group_member) == min(group_member):
                    height_scaling = 1
                else:
                    height_scaling = (HEIGHT-20) / (max(max(group_member),-min(group_member)) * 2)
                
                if group_scalings[group_name][0] is None or width_scaling < group_scalings[group_name][0]:
                    group_scalings[group_name][0] = width_scaling
                if group_scalings[group_name][1] is None or height_scaling < group_scalings[group_name][1]:
                    if max(group_member) != min(group_member):
                        group_scalings[group_name][1] = height_scaling                
            
        
        for idx, (name, arr) in enumerate(y_data_s.items()):
            width_scaling, height_scaling = get_scaling(group_scalings, arr, name)
            if not isinstance(arr, list):
                arr = [arr]
            if len(arr) == 1:
                arr.append(arr[0])
            color = colors[idx % len(colors)]
            for x, y in enumerate(arr):
                if x < len(arr) - 1:
                    cv2.line(canvas, (int(x * width_scaling), int(arr[x] * height_scaling + HEIGHT/2),),
                                     (int((x + 1) * width_scaling), int(arr[x + 1] * height_scaling + HEIGHT/2)),
                                     color, 2)
                cv2.circle(canvas, (int(x * width_scaling), int(y * height_scaling + HEIGHT/2)), 6, color, -1)
        
        cv2.imshow("plot", canvas)
        cv2.waitKey(50)

app = FastAPI()

@app.post("/plot_data")
async def plot_data(_data: dict):
    global data
    data = _data
    return "ok", 200

import threading

def start_cv2():
    while True:
        animate()
threading.Thread(target=start_cv2).start()

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5001)
    