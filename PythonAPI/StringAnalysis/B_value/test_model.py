import numpy as np
from tensorflow.keras.models import load_model
from tensorflow.keras.preprocessing.sequence import pad_sequences
import soundfile as sf
from os import path
import usefull_audioStuff
import B_formula


model_path = 'B_value_model.h5'
model = load_model(model_path)

audio, string_name, samplerate = usefull_audioStuff.extract_audio_and_string_names_from_onefile(path.join("test","highEonallstrings.mp3"))
string_name = string_name.split("\\")[1]
# Print the model's input shape
print("Model input shape:", model.input_shape)
audios = {}
audios[string_name] = audio
notes = usefull_audioStuff.get_audio_whith_peak(samplerate, audios)
optimal_b_values = {}
all_amplitude_ratios = {}
i = 0
for note_stringname, audio_clip in notes:
    i += 1
    #inharmonicity
    current_stringname = str(string_name + str(i))
    frequency_peaks, amplitude_peaks = usefull_audioStuff.calc_freq_peaks(audio_clip, samplerate)
    freq_data = [(i+1,float(peak)) for i,peak in enumerate(frequency_peaks)]
    length = len(frequency_peaks)
    f1 = freq_data[0][1] if freq_data else 0
    optimal_b = B_formula.binary_search_optimize(length, freq_data, f1)
    if current_stringname not in optimal_b_values:
        optimal_b_values[current_stringname]= []
    optimal_b_values[current_stringname].append(optimal_b)
    #ratio of amplitude

    fund_amplitude = amplitude_peaks[0]
    amplitude_ratios = [amplitude / fund_amplitude for amplitude in amplitude_peaks]
    average_amplitude_ratio = sum(amplitude_ratios) / len(amplitude_ratios)
    if current_stringname not in all_amplitude_ratios:
        all_amplitude_ratios[current_stringname]= []
    all_amplitude_ratios[current_stringname].append(average_amplitude_ratio)




# Create a list of tuples (amplitude_ratio, b_value)
b_value_amplitude_pairs = []
for key in all_amplitude_ratios:
    for amplitude_ratio, b_value in zip(all_amplitude_ratios[key], optimal_b_values[key]):
        b_value_amplitude_pairs.append(( b_value,amplitude_ratio))


print(b_value_amplitude_pairs)
for b_value_amplitude_pair in b_value_amplitude_pairs:

    max_len = 158
    pair = np.array(b_value_amplitude_pair, dtype='float32')
    pair = pair[:max_len]  # Ensure the pair array does not exceed max_len
    new_data = np.zeros((1, max_len, 2), dtype='float32')
    new_data[0, :pair.shape[0], :] = pair

    new_data_padded = pad_sequences(new_data, maxlen=158, dtype='float32', padding='post', truncating='post')
    new_data_padded = np.expand_dims(new_data_padded,-1)

    # Predict results
    new_predictions = model.predict(new_data_padded)


    predicted_classes = np.argmax(new_predictions, axis=1)

    print("Predicted probabilities:", new_predictions)
    print("Predicted class:", predicted_classes)
