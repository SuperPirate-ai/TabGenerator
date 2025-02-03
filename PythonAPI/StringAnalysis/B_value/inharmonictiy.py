import numpy as np
def calculate_Inharmonicity(fk_values,f0):
    
    #f0 = 110
    #fk_values = [110, 222, 331, 442]
    k_values = [1, 2, 3, 4]

    # Compute (fk/f0)^2 for each fk
    lhs = [(fk / f0) ** 2 for fk in fk_values]

    # Construct the Vandermonde matrix for k values
    A = np.array([[k**i for i in range(5)] for k in k_values])

    # Solve for p0, p1, p2, p3, p4
    p_coeffs = np.linalg.lstsq(A, lhs, rcond=None)[0]

    # # Print results
    # sum = 0
    # for i, p in enumerate(p_coeffs):
    #     print(f"p{i} = {p:.6f}")
    #     sum += p

    # print(f"Sum of p values: {sum:.6f}")
    return p_coeffs

