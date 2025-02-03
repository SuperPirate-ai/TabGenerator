import usefull_audioStuff
import json

audio, string_names, samplerate = usefull_audioStuff.extract_audio_and_string_names()
notes = usefull_audioStuff.get_audio_whith_peak(samplerate, audio)

avg_amplitude_ratios = {}
for stringname, audio_clip in notes:
    frequency_peaks, amplitude_peaks = usefull_audioStuff.calc_freq_peaks(audio_clip, samplerate)
    if len(amplitude_peaks) == 0:
        continue
    fund_amplitude = amplitude_peaks[0]
    amplitude_ratios = [amplitude / fund_amplitude for amplitude in amplitude_peaks]
    average_amplitude_ratio = sum(amplitude_ratios) / len(amplitude_ratios)

    if stringname not in avg_amplitude_ratios:
        avg_amplitude_ratios[stringname] = []
    avg_amplitude_ratios[stringname].append(average_amplitude_ratio)

print(avg_amplitude_ratios)

#save to json file 
with open('avg_amplitude_ratios.json', 'w') as file:
    json.dump(avg_amplitude_ratios, file, indent=4)