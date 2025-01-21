from copy import deepcopy

def rec_count(pattern, pos):
    if pos >= len(pattern):
        return pattern

    if pattern[pos] == 3:
        pattern[pos] = 0
        return rec_count(pattern, pos + 1)
    else:
        pattern[pos] += 1
        print(pattern)
        return rec_count(pattern, 0)

# Example usage
pattern = [0, 0, 0, 0]
rec_count(pattern, 0)
