import numpy as np
import soundfile as sf
# Description: This script will take the raw samples from the _rawSamples.csv file and convert them into an mp3 file.

with open ("_rawSamples.csv", "r") as f:
    reader = f.read()

samples = reader.split(",")
samples = np.array([float(sample) for sample in samples])

sf.write("rawSamples.mp3", samples, 44100)


