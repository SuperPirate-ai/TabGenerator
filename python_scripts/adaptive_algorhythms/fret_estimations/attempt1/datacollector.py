import pandas as pd
from fastapi import FastAPI
import numpy as np
from keras.models import load_model
import requests
from sbNative.runtimetools import get_path

global df
df = None
app = FastAPI()

@app.post("/save_data")
async def save_data(_data: dict):
    global df
    d_keys = list(_data.keys())
    d_keys.remove("fft_arr")
        
    if df is None:        
        df =  pd.DataFrame(columns=d_keys + list(range(0, 500)))
        
    df.loc[df.shape[0]] = [_data[k] for k in d_keys] + _data["fft_arr"]
    
    return "ok", 200

@app.get("/put_data_in_file")
async def put_data_in_file():
    global df
    df.to_csv("data.csv")


# Load the model from the file
loaded_model = load_model(get_path() / 'fret_position_model.h5')

# Make predictions


def idxmax(arr):
    _max = 0
    idx = -1
    for i in range(1, len(arr)):
        if arr[i] > _max:
            _max = arr[i]
            idx = i
    return idx

@app.post("/predict")
async def test_data(_data: dict):
    global loaded_model
    global df
    fft_arr = np.array(_data["fft_arr"])
    print(fft_arr)
    raw_prediction = loaded_model.predict(fft_arr.reshape(-1, fft_arr.shape[0]))[0]
    prediction = idxmax(raw_prediction)
    print(raw_prediction, prediction)
    requests.post("http://localhost:5001/plot_data", json={"y_data_s": {"fret": [float(x) for x in raw_prediction]}, "time": -1, "common_scaling_groups": []})
    
    return "ok", 200



if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5002)