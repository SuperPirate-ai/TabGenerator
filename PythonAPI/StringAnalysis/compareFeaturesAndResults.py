from os import path
import os
with open(path.join(os.getcwd(),"results","results.csv"), "r") as f:
    results = f.readlines()
    results = [x.strip().split(",") for x in results]
    results = [(float(x[1]), float(x[2]), float(x[3]), float(x[4]), x[0]) for x in results]
    #delete all where x[0] is not A
    results = [x for x in results if x[4] == "A"]
    #cut off all x[4]
    results = [(x[0], x[1], x[2], x[3]) for x in results]

with open(path.join(os.getcwd(),"results","features.csv"), "r") as f:
    features = f.readlines()
    features = [x.strip().split(",") for x in features]
    features = [(float(x[0]), float(x[1]), float(x[2]), float(x[3])) for x in features]


print("Results:")
print(len(results))
print("Features:")
print(len(features))

#go through all features and compare them to the results if they don't match print them in red
for i in range(len(features)):
    if features[i] != results[i]:
        print(f"Feature {i} does not match")
        print(f"Results: {results[i]}")
        print(f"Features: {features[i]}")
        print("\n")
    else:
        print(f"Feature {i} matches")
        print("\n")