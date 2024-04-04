from keras.models import Sequential
from keras.layers import Dense, Flatten, Conv2D, MaxPooling2D
from keras.optimizers import Adam
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler


# Load the data
df = pd.read_csv('first_training_data_24outneurons.csv')

x = df.drop(columns=["Unnamed: 0"] + [f"Fret {idx}" for idx in range(0,25)])
y = df[[f"Fret {idx}" for idx in range(0,25)]]

# Normalize input features
scaler = StandardScaler()
x = scaler.fit_transform(x)

# Split the data into training, validation, and test sets
x_train, x_test, y_train, y_test = train_test_split(x, y, test_size=0.2)

print(x_train.shape, x_test.shape)
# Define your neural network model
model = Sequential()
model.add(Dense(512*2, activation='relu', input_shape=x_train.shape[1:]))
model.add(Dense(512, activation='relu'))
model.add(Dense(256, activation='relu'))
model.add(Dense(25, activation="softmax"))

# Compile the model
model.compile(optimizer=Adam(lr=0.001), loss='categorical_crossentropy', metrics=['accuracy'])


model.fit(x_train, y_train, epochs=50, batch_size=4, shuffle=True)

# Evaluate the model
test_loss = model.evaluate(x_test, y_test)
model.save('fret_position_model.h5')