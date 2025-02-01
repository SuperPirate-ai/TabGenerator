import numpy as np
import scipy.fft
import soundfile as sf
import os
import json
import B_formula


print(os.getcwd())
for filename in os.listdir(os.getcwd()):
    if filename.endswith(".mp3"):
        print("filename",filename)
        audio, sample_rate = sf.read(filename)
        print(sample_rate)
        # Convert stereo to mono if necessary
        

        # Ensure the audio file has at least 6 seconds of audio
        assert len(audio) >= 20 * sample_rate, "The audio file is shorter than 6 seconds."

        # Split audio into 6 buffers, each 1 second long
        buffers = [audio[i * sample_rate:(i + 1) * sample_rate] for i in range(25*6)]
        
        # Take the first 4096 samples of each buffer
        short_buffers = [buffer[:4096*2] for buffer in buffers]
        optimal_bs = []
        # Perform Fourier analysis and create separate canvases
        def save_sample_data(buffer, buffer_index):
            if len(buffer) == 0:
                raise ValueError("Buffer is empty, cannot perform FFT.")
            # Fourier transform
            fft_result = scipy.fft.fft(buffer)
            magnitudes = np.abs(fft_result)
            frequencies = scipy.fft.fftfreq(len(buffer), d=1/sample_rate)  # Frequency in Hz

            # Only keep the positive frequencies up to 2000 Hz
            positive_frequencies = frequencies[:len(buffer) // 2]
            positive_magnitudes = magnitudes[:len(buffer) // 2]
            limit_index = np.where(positive_frequencies <= 2000)[0]
            plot_frequencies = positive_frequencies[limit_index]
            plot_magnitudes = positive_magnitudes[limit_index]

            frequency_peaks = []
            peak_amplitudes = []

            for i in range(2, len(plot_magnitudes) - 2):
                if plot_magnitudes[i] > 10 and plot_magnitudes[i] > plot_magnitudes[i - 1] and plot_magnitudes[i] > plot_magnitudes[i + 1] and plot_magnitudes[i] > plot_magnitudes[i - 2] + plot_magnitudes[i + 2]:
                    frequency_peaks.append(plot_frequencies[i])
                    peak_amplitudes.append(plot_magnitudes[i])
            if len(frequency_peaks) != 0:
                data = [(i+1,float(peak)) for i,peak in enumerate(frequency_peaks)]
                length = len(frequency_peaks)
                f1 = data[0][1] if data else 0
                optimal_b = B_formula.binary_search_optimize(length, data, f1)
                optimal_bs.append(optimal_b)
                print(f"Buffer {buffer_index + 1}: {optimal_b}")

        for i, buffer in enumerate(short_buffers):
            if len(buffer) > 0:
                save_sample_data(buffer, i)
            else:
                print(f"Buffer {i+1} is empty, skipping FFT.")
            
        with open(filename.replace(".mp3",".json"), "w", encoding='utf-8') as f:
            f.write(json.dumps({"optimal_bs":optimal_bs}, indent=4))
        


