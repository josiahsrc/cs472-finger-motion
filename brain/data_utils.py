import pandas as pd
import numpy as np
import os

from sklearn.neural_network import MLPRegressor
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import normalize
from sklearn.experimental import enable_iterative_imputer
from sklearn.impute import IterativeImputer

def score_avg_mse(model, X_test, y_test):
    tot = 0.0
    for _ in range(10):
        tot += model.score(X_test, y_test)
    return tot / 10

# Saves a numpy array to a CSV file, overwriting
# anything that was there.
def save_np_array_to_csv(array, path2csv):
    dir_name = os.path.dirname(path2csv)
    if not os.path.exists(dir_name):
        os.makedirs(dir_name)
    np.savetxt(path2csv, array, delimiter=",")

# This method prepares data by imputing mssing values
# by learning patterns in the dataset. If missing datapoints
# are found, a learner will predict was is missing.
# 
# The data will then be normalized in L2.
def prepare_data_imputed_norm(X, y):
    # Impute missing values
    X = IterativeImputer(max_iter=10).fit(X).transform(X)
    y = IterativeImputer(max_iter=10).fit(y).transform(y)
    
    # Normalize the data
    X, y = normalize(X, norm='l2'), normalize(y, norm='l2')
    
    return X, y
