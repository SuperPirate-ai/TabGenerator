import datetime as dt
import matplotlib.pyplot as plt
from fastapi import FastAPI
import threading

# Create figure for plotting
fig = plt.figure()
ax = fig.add_subplot(1, 1, 1)

# Initialize communication with TMP102

# This function is called periodically from FuncAnimation
def animate(y, xs):

    # Read temperature (Celsius) from TMP102


    # Draw x and y lists
    ax.clear()
    for x in xs:
        ax.plot(y, x)

    # Format plot
    plt.xticks(rotation=45, ha='right')
    plt.subplots_adjust(bottom=0.30)
    plt.title(dt.datetime.now().strftime('%H:%M:%S.%f'))
    plt.pause(0.01)


app = FastAPI()

@app.post("/plot_data")
async def plot_data(data: dict):
    
    animate(data["y"], data["xs"])
    return "ok", 200
    

if __name__ == "__main__":
    
    import uvicorn
    print("running")
    uvicorn.run(app, host="0.0.0.0", port=5001)