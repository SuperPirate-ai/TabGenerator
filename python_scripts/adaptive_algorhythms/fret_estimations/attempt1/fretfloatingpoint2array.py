import pandas as pd

# Load the data
df = pd.read_csv('first_training_data.csv')

dfout = pd.DataFrame(columns=[f"Fret {idx}" for idx in range(0,25)] + list(range(0, 775)))

for i in range(df.shape[0]):
    row = df.iloc[i]
    fret_floating_point = row['fret_floating_point']
    fft_arr = row.drop(['fret_floating_point', "Unnamed: 0"])
    new_row = [0]*25
    new_row[int(fret_floating_point*25-1)] = 1
    new_row += list(fft_arr)
    dfout.loc[dfout.shape[0]] = new_row
    
dfout.to_csv('data.csv')
    