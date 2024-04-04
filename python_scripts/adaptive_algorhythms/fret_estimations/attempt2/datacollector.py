import pandas as pd
from fastapi import FastAPI
import numpy as np
from keras.models import load_model
import requests
from sbNative.runtimetools import get_path
from sklearn.preprocessing import StandardScaler
import joblib

global filename
global df
global counter
filename = "data.csv"
df = None

app = FastAPI()

counter = 0

@app.post("/save_data")
async def save_data(_data: dict):
    global df
    global filename
    global counter
    counter += 1
    print(counter)
    first_values = df is None
    if first_values:  
        df =  pd.DataFrame(columns=list(_data.keys()))
        
    df.loc[df.shape[0]] = list(_data.values())
    
    if first_values:
        df.to_csv(filename, index=False)
    else:
        df.to_csv(filename, index=False, header=False)
    return 200

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
    in_keys = ["frequency","maxlevel","overtone1level","overtone2level","overtone3level","overtone4level","overtone5level"]
    in_arr = np.array([[_data[k] for k in in_keys]])
    x = loaded_scaler.transform(in_arr)
    raw_prediction = loaded_model.predict(x)[0]
    prediction = idxmax(raw_prediction)
    print(prediction)
    # requests.post("http://localhost:5001/plot_data", json={"y_data_s": {"fret": [float(x) for x in raw_prediction]}, "time": -1, "common_scaling_groups": []})

    return f"{prediction}"



if __name__ == "__main__":
    loaded_model = load_model(get_path() / 'guitar_string_model.h5')
    loaded_scaler = joblib.load(get_path() / 'guitar_string_scaler.pkl')
    try:
        df = pd.read_csv(filename, index_col=False)
    except FileNotFoundError:
        df = None
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5002)