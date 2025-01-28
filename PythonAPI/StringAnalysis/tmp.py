import numpy as np
import scipy.fft
import soundfile as sf
import matplotlib.pyplot as plt
import os
import json

# Load the audio file
#filename = "thin_E_all_notes.mp3"  # Replace with your file
#get all files in this directory that and with .mp3
print(os.getcwd())
for filename in os.listdir(os.getcwd()):
    if filename.endswith(".mp3"):
        audio, sample_rate = sf.read(filename)
        print(sample_rate)
        # Ensure the audio file has at least 6 seconds of audio
        assert len(audio) >= 20 * sample_rate, "The audio file is shorter than 6 seconds."

        # Split audio into 6 buffers, each 1 second long
        buffers = [audio[i * sample_rate:(i + 1) * sample_rate] for i in range(20)]

        # Take the first 4096 samples of each buffer
        short_buffers = [buffer[:4096*2] for buffer in buffers]
        global metric_data
        metric_data = ""
        # Perform Fourier analysis and create separate canvases
        def create_plot(buffer, buffer_index):
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
            if len(frequency_peaks) == 0:
                return 0    
            calucated_frequency_peaks = []
            for i in range(1, 6):
                calucated_frequency_peaks.append(frequency_peaks[0] * i)
                
            metric_1 = 0
            for a, f in zip(peak_amplitudes, frequency_peaks):
                metric_1 += a * f
            metric_1 /= sum(peak_amplitudes)
            print(f"Buffer {buffer_index + 1}: {metric_1:.1f} db/s")  
            global metric_data 
            metric_data += str(metric_1) + "\n"
            # Create a separate figure
            fig, ax = plt.subplots(figsize=(10, 6))
            line, = ax.plot(plot_frequencies, plot_magnitudes, label=f"Buffer {buffer_index + 1}")
            ax.plot(frequency_peaks, peak_amplitudes, "ro", label="Peaks")
            for i in range(1, 6):
                ax.axvline(x=calucated_frequency_peaks[i - 1], color='g', linestyle='--', label=f"Peak {i}: {calucated_frequency_peaks[i - 1]:.1f} Hz")
            ax.axvline(x=metric_1, color='b', linestyle='--', label=f"Metric 1: {metric_1:.1f} db/s")
            ax.set_title(f"Fourier Transform of Buffer {buffer_index + 1}")
            ax.set_xlabel("Frequency (Hz)")
            ax.set_ylabel("Magnitude")
            ax.grid()
            ax.set_xlim(0, 2000)
            ax.legend()

            # Annotation for hover info
            annot = ax.annotate("", xy=(0, 0), xytext=(20, 20),
                                textcoords="offset points",
                                bbox=dict(boxstyle="round", fc="w"),
                                arrowprops=dict(arrowstyle="->"))
            annot.set_visible(False)

            # Function to update annotation
            def update_annot(ind):
                x, y = line.get_data()
                annot.xy = (x[ind["ind"][0]], y[ind["ind"][0]])
                text = f"{x[ind['ind'][0]]:.1f} Hz\n{y[ind['ind'][0]]:.2f}"
                annot.set_text(text)
                annot.get_bbox_patch().set_alpha(0.8)

            # Event handler for hovering
            def on_hover(event):
                vis = annot.get_visible()
                if event.inaxes == ax:
                    cont, ind = line.contains(event)
                    if cont:
                        update_annot(ind)
                        annot.set_visible(True)
                        fig.canvas.draw_idle()
                    else:
                        if vis:
                            annot.set_visible(False)
                            fig.canvas.draw_idle()

            # Connect the event
            fig.canvas.mpl_connect("motion_notify_event", on_hover)
            return metric_1

        average_metric_1 = 0
        # Create and show plots for each buffer
        for i, buffer in enumerate(short_buffers):
            average_metric_1 += create_plot(buffer, i)
        average_metric_1 /= len(short_buffers)
        print(f"Average Metric 1: {average_metric_1:.1f} db/s")

        with open(f"{filename.replace('.mp3', '')}.json", "w") as file:
            file.write(json.dumps({f"{filename}":metric_data}))
        # Show all plots simultaneously
        #plt.show()
