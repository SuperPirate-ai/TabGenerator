from keras.models import Sequential
from keras.layers import Dense, Flatten, Conv2D, MaxPooling2D
from keras.optimizers import Adam
import pandas as pd
from sklearn.model_selection import train_test_split

# Load the data
df = pd.read_csv('first_training_data.csv')

x = df.drop(columns=['fret_floating_point', "Unnamed: 0"])
y = df['fret_floating_point']

# Split the data into training, validation, and test sets
x_train, x_test, y_train, y_test = train_test_split(x, y, test_size=0.2)
# Define your neural network model
model = Sequential()
model.add(Dense(512, activation='relu', input_shape=(x_train.shape[1],)))
model.add(Dense(256, activation='relu'))
model.add(Dense(256, activation='relu'))
model.add(Dense(256, activation='relu'))
model.add(Dense(128, activation='relu'))
model.add(Dense(64, activation='relu'))
model.add(Dense(64, activation='relu'))
model.add(Dense(32, activation='relu'))
model.add(Dense(16, activation='relu'))
model.add(Dense(1))

# Compile the model
model.compile(optimizer=Adam(lr=0.001), loss='mean_squared_error')

# Train the model
model.fit(x_train, y_train, epochs=500, batch_size=32, shuffle=True, steps_per_epoch=50)

# Evaluate the model
test_loss = model.evaluate(x_test, y_test)
print(f'Test Loss: {test_loss}')
model.save('fret_position_model.h5')