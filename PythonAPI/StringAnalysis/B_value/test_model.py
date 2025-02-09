import onnx
onnx_model = onnx.load("modelData.onnx")
onnx.checker.check_model(onnx_model)
#print inputshape
print(onnx_model.graph.input)
## test the model with numbers
import onnxruntime as rt
import numpy as np
import pandas as pd
from colorama import Fore, Style

sess = rt.InferenceSession("modelData.onnx")
input_name = sess.get_inputs()[0].name
label_name = sess.get_outputs()[0].name

vals = pd.read_csv('testresults.csv').values

strings = ["h_E", "B", "G", "D", "A", "E"]
labels = vals[:, 0]
data = vals[:, 1:]
for d, l in zip(data, labels):
    d = d.reshape(1, -1).astype(np.float32)
    result = strings[np.argmax(sess.run([label_name], {input_name: d})[0][0])]

    if l == result:
        print(f"{Fore.GREEN}expected: {l}, got: {result}{Style.RESET_ALL}")
    else:
        print(f"{Fore.RED}expected: {l}, got: {result}{Style.RESET_ALL}")
 