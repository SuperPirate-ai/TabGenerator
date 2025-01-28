import numpy as np
from tensorflow.keras.models import load_model

# Load the model
import os
model_path = 'B_value_model.h5'
model = load_model(model_path)
predefined_input = np.array([[0.5, 0.2, 0.1, 0.7, 0.3]])
# Print the model's input shape
print("Model input shape:", model.input_shape)

# Ensure the input shape matches the model's expected input shape



# Define a predefined input
predefined_input = (None, 0.00024242991741872617)

# Make a prediction
prediction = model.predict(predefined_input)

# Print the prediction
print("Prediction:", prediction)