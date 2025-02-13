import onnx
from os import path
model_path = path.join("models","stringDetectionML_ONNX.onnx")
onnx_model = onnx.load(model_path)
onnx.checker.check_model(onnx_model)
#print inputshape
#print(onnx_model.graph.input)


for tensor in onnx_model.graph.initializer:
    assert tensor.data_type == onnx.TensorProto.FLOAT, f"{tensor.name} is not FP32!"
print("All tensors are FP32")



## test the model with numbers
import onnxruntime as rt
import numpy as np
import pandas as pd
from colorama import Fore, Style

sess = rt.InferenceSession(model_path,providers=['CPUExecutionProvider'])
sess.disable_fallback()
input_name = sess.get_inputs()[0].name
label_name = sess.get_outputs()[0].name

vals = pd.read_csv(path.join("results",'testresults.csv')).values

strings = ["h_E", "B", "G", "D", "A", "E"]
labels = vals[:, 0]
data = vals[:, 1:]
predicted_right = 0
predicted_wrong = 0
i = 0
for d, l in zip(data, labels):
    d = d.reshape(1, -1).astype(np.float32)
    result = strings[np.argmax(sess.run([label_name], {input_name: d})[0][0])]

    if l == result:
        print(f"{Fore.GREEN}expected: {l}, got: {result}{Style.RESET_ALL}")
        predicted_right += 1
    else:
        print(f"{Fore.RED}expected: {l}, got: {result}{Style.RESET_ALL}")
        predicted_wrong += 1

    if i == 0:
        probabilities = sess.run([label_name], {input_name: d})[0][0]
        for string, probability in zip(strings, probabilities):
            print(f"{string}: {probability:.4f}")
        i += 1
print(f"predicted right: {predicted_right}, predicted wrong: {predicted_wrong}")
 