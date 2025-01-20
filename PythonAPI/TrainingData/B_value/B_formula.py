import math

def read_file(file_path):
    with open(file_path, 'r') as file:
        lines = file.readlines()
        n = int(lines[0].strip())
        data = [(i+1,float(line.strip())) for i,line in enumerate(lines[1:])]
    return n, data

def show_prediction(b, n, data, f1):
    total_error = 0
    for ni, fn in data:
        predicted = (
            ni * f1 * math.sqrt(1 + b * ni**2) * 
            (1 + 2 / math.pi * math.sqrt(b) + 4 / math.pi * b)
        )
        print(f"Predicted: {predicted}, Actual: {fn}")
        total_error += ((predicted - fn)**2)
        
    print(f"Total error: {total_error}")
    return total_error

def calculate_r2(b, n, data, f1):
    observed = [fn for _, fn in data]
    mean_observed = sum(observed) / len(observed)
    
    ss_res = 0
    ss_tot = 0
    for ni, fn in data:
        predicted = (
            ni * f1 * math.sqrt(1 + b * ni**2) * 
            (1 + 2 / math.pi * math.sqrt(b) + 4 / math.pi * b)
        )
        ss_res += (predicted - fn)**2
        ss_tot += (fn - mean_observed)**2
    
    r2 = 1 - (ss_res / ss_tot)
    return r2

def error_function(b, n, data, f1):
    total_error = 0
    for ni, fn in data:
        predicted = (
            ni * f1 * math.sqrt(1 + b * ni**2) * 
            (1 + 2 / math.pi * math.sqrt(b) + 4 / math.pi * b)
        )
        total_error += (predicted - fn)**2

    return total_error

def binary_search_optimize(n, data, f1, lower=0, upper=100, tol=1e-6):
    while upper - lower > tol:
        mid1 = lower + (upper - lower) / 3
        mid2 = upper - (upper - lower) / 3
        err1 = error_function(mid1, n, data, f1)
        err2 = error_function(mid2, n, data, f1)
        
        if err1 < err2:
            upper = mid2
        else:
            lower = mid1
    return (lower + upper) / 2

# Main script
if __name__ == "__main__":
    file_path = "E_0"  # Replace with your file path
    n, data = read_file(file_path)
    
    # Assuming the first value corresponds to f(1)
    f1 = data[0][1] if data else 0
    
    # Optimize to find the best b
    optimal_b = binary_search_optimize(n, data, f1)
    print(f"Optimal b: {optimal_b}")
    show_prediction(optimal_b, n, data, f1)
    print(calculate_r2(optimal_b, n, data, f1))
