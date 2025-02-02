import numpy as np
import scipy.fft
import soundfile as sf
import os

# Load the audio files and store them in a dictionary
def calc_freq_peaks(audio_clip, samplerate):

    fft_data = scipy.fft.fft(audio_clip)
    amplitudes = np.abs(fft_data)
    frequencies = np.fft.fftfreq(len(fft_data), 1/samplerate)
    positive_frequencies = frequencies[:len(frequencies)//2]
    positive_amplitudes = amplitudes[:len(amplitudes)//2]
    limit_index = np.where(positive_frequencies <= 2000)[0]
    
    plot_frequencies = positive_frequencies[limit_index]
    plot_amplitudes = positive_amplitudes[limit_index]
    
    frequency_peaks = []
    peak_amplitudes = []

    
    for i in range(2, len(plot_amplitudes) - 2):
        if plot_amplitudes[i] > 4 and plot_amplitudes[i] > plot_amplitudes[i - 1] and plot_amplitudes[i] > plot_amplitudes[i + 1] and plot_amplitudes[i] > plot_amplitudes[i - 2] + plot_amplitudes[i + 2]:
            frequency_peaks.append(plot_frequencies[i])
            peak_amplitudes.append(plot_amplitudes[i])

   
    return frequency_peaks, peak_amplitudes

def extract_audio_and_string_names():
    audios = {}
    string_names = []
    for filename in os.listdir(os.getcwd()):
        if filename.endswith(".mp3"):
            audio, samplerate = sf.read(filename)
            string_name = filename.split(".")[0].replace("_string", "")
            #print(string_name)
            audios[string_name] = audio
            if string_name not in string_names:
                string_names.append(string_name)
            else:
                break
    return audios, string_names,samplerate
def extract_audio_and_string_names_from_onefile(filename):
    
    if filename.endswith(".mp3"):
            audio, samplerate = sf.read(filename)
            string_name = filename.split(".")[0].replace("_string", "")
            
    return audio, string_name,samplerate
    
def get_audio_whith_peak(samplerate, audios):
    notes = []
    BUFFERSIZE = 8192
    for stringname, audio in audios.items():
        half_wavelength_for_50_hz = samplerate // 100
        loudness = []
        for i in range(0, len(audio), half_wavelength_for_50_hz):
            loudness.append(np.max(np.abs(audio[i:i+half_wavelength_for_50_hz])))
        

        new_note_indecies = []
        for i in range(0, len(loudness)):
            if 0.015 <  loudness[i] > loudness[i-1] * 2.5:
                new_note_indecies.append(i)

        print(len(new_note_indecies))
        for index in new_note_indecies:
            notes.append((stringname, audio[index*half_wavelength_for_50_hz : index*half_wavelength_for_50_hz+BUFFERSIZE]))
    return notes

