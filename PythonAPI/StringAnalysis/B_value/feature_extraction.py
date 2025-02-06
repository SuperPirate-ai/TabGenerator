import numpy as np
import os
import scipy.fft
import soundfile as sf


audios = {}
stringnames = []

for mp3 in os.listdir("."):
    if not "_string.mp3" in mp3:
        continue
    
    print(mp3)
    data, fs = sf.read(mp3)
    stringname = mp3.replace("_string.mp3", "")
    audios[stringname] = data
    if stringname not in stringnames:
        stringnames.append(stringname)

BUFSIZE = 8192
notes = [] ## string, data
for stringname, audio in audios.items():
    # get loudness over time of audio
    half_wavelength_for_50_hz = fs // 100
    loudness = []
    for i in range(0, len(audio), half_wavelength_for_50_hz):
        loudness.append(np.max(np.abs(audio[i:i+half_wavelength_for_50_hz])))

    new_note_indecies = []
    for i in range(1, len(loudness)):
        if 0.02 < loudness[i] > loudness[i-1] * 2.5:
            new_note_indecies.append(i)
    print(len(new_note_indecies))

    
    for index in new_note_indecies:
        notes.append((stringname, audio[index*half_wavelength_for_50_hz:index*half_wavelength_for_50_hz+BUFSIZE]))

# stringnames = ["E", "A", "D", "G", "B", "h_E"]







results = [] # metric, freq, clip_index, mp3



for stringname, clip in notes:
    # Fourier transform
    fft_result = scipy.fft.fft(clip)
    magnitudes = np.abs(fft_result)
    frequencies = scipy.fft.fftfreq(len(clip), d=1/fs)  # Frequency in Hz
    positive_frequencies = frequencies[:len(clip) // 2]
    positive_magnitudes = magnitudes[:len(clip) // 2]
    limit_index = np.where(positive_frequencies <= 2000)[0]
    
    plot_frequencies = positive_frequencies[limit_index]
    plot_magnitudes = positive_magnitudes[limit_index]
    
    frequency_peaks = []
    amplitude_peaks = []

    
    for i in range(2, len(plot_magnitudes) - 2):
        if plot_magnitudes[i] > 4 and plot_magnitudes[i] > plot_magnitudes[i - 1] and plot_magnitudes[i] > plot_magnitudes[i + 1] and plot_magnitudes[i] > plot_magnitudes[i - 2] + plot_magnitudes[i + 2]:
            frequency_peaks.append(plot_frequencies[i])
            amplitude_peaks.append(plot_magnitudes[i])

    if len(amplitude_peaks) == 0:
        print(f"No peaks found for {stringname}")
        continue
    
    real_base_freq = frequency_peaks[0]
    overtone_amplitudes = {} # overtone index -> amplitude
    overtone_frequenices = {} # overtone index -> frequency
    for overtone_freq, amplitude in zip(frequency_peaks, amplitude_peaks):    
        base_to_overtone_factor = round(overtone_freq / real_base_freq)
        real_base_freq = overtone_freq / base_to_overtone_factor
        overtone_index = base_to_overtone_factor - 1
        if overtone_index not in overtone_amplitudes:
            overtone_amplitudes[overtone_index] = float(amplitude)
            overtone_frequenices[overtone_index] = [overtone_freq]
        else:
            overtone_amplitudes[overtone_index] += float(amplitude)
            overtone_frequenices[overtone_index].append(overtone_freq)
    
    overtone_frequenices = {i: sum(overtone_frequenices[i]) / len(overtone_frequenices[i]) for i in overtone_amplitudes.keys()}

    #metric
    amplitude_times_frequencies = []
    for a, f in zip(amplitude_peaks, frequency_peaks):
        amplitude_times_frequencies.append(a * f)
        
    metric_1 = 1/(sum(amplitude_times_frequencies) / sum(amplitude_peaks))
    amplitude_ratios = []


    metric_2 = overtone_amplitudes.get(0, 0) - overtone_amplitudes.get(1, 1)
    metric_2 *= .0001

    #amplitude ratio
    for amp in amplitude_peaks:
        amplitude_ratios.append(amp / amplitude_peaks[0])
    amplitude_ratio = sum(amplitude_ratios) / len(amplitude_ratios)
    amplitude_ratio *= .0001

    #  deviation 
    f0 = real_base_freq
    
    deviations = []
    for overtone_index, overtone_freq in overtone_frequenices.items():
        expected_freq = f0 * (overtone_index + 1)
        deviation = abs(overtone_freq / expected_freq)
        deviations.append(deviation)
    

    avg_deviation = sum(deviations) / len(deviations)
    #print(f"{stringname} {metric_1 = } {metric_2 = } {amplitude_ratio = } {avg_deviation = } {real_base_freq = }")
    results.append((metric_1 + metric_2, amplitude_ratio,avg_deviation ,real_base_freq, stringname))

   

csv_text = ""
for metric, amp_ra, deviation,freq, stringname in results:
    csv_text += f"{stringname},{metric:.20f},{amp_ra:.20f},{deviation:.20f},{freq:.5f}\n"


with open("results.csv", "w") as f:
    f.write(csv_text)