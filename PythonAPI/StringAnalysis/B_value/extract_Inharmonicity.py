import numpy as np
import scipy.fft
import soundfile as sf
import os
import json
import B_formula
import usefull_audioStuff
# Load the audio files and store them in a dictionary
audios, string_names, samplerate = usefull_audioStuff.extract_audio_and_string_names()

# get all indexes where there is a new note
notes = usefull_audioStuff.get_audio_whith_peak(samplerate, audios)

#take the fft of the audio clips where there is a new note and find the peaks
optimal_bs = []
for stringname, audio_clip in notes:
    frequency_peaks, amplitude_peaks = usefull_audioStuff.calc_freq_peaks(audio_clip, samplerate)
    
    #calculate the optimal b values for each string
    freq_data = [(i+1,float(peak)) for i,peak in enumerate(frequency_peaks)]
    length = len(frequency_peaks)
    f1 = freq_data[0][1] if freq_data else 0
    optimal_b = B_formula.binary_search_optimize(length, freq_data, f1)
    optimal_bs.append((stringname, optimal_b))



# save all b values of one string in a seperate dict
b_values = {}
for stringname, optimal_b in optimal_bs:
    if stringname not in b_values:
        b_values[stringname] = []
    b_values[stringname].append(optimal_b)

for stringname, b_value in b_values.items():
    print(f"{stringname}: {len(b_value)}")


#make a graph where the x-axis is the string name and the y-axis is the b value
import matplotlib.pyplot as plt
# for stringname, b_value in b_values.items():
#     plt.bar(stringname, np.average(b_value))

#go through all b values and count how often diffrent b_values allign in in different strings
b_value_counts = {}

for stringname, b_value in b_values.items():
    local_b_value_counts = {}
    for b in b_value:
        if b not in b_value_counts:
            b_value_counts[b] = 0
            local_b_value_counts[b] = 0
        elif b not in local_b_value_counts:
            b_value_counts[b] += 1

#make a graph where the x-axis is the b value and the y-axis is the count. only take bvalues that are in more than one string
# b_value_counts = {b:count for b,count in b_value_counts.items() if count > 5}
# plt.bar(b_value_counts.keys(), b_value_counts.values())

for stringname, b_value in b_values.items():
    if stringname == "highEonallstrings":
        print(b_value)
        #plot all b values of the string on the Y-axis and the index of the b value on the x-axis
        plt.plot(range(len(b_value) - 1), b_value[:-1])
        

plt.show()
# save the b values in a json file. Seperate the b values of each string
with open("b_values.json", 'w') as f:
    json.dump(b_values, f, indent=4)

