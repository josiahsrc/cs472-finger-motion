import setup

import warnings
import pandas as pd
import numpy as np
import data_utils as dutils

from sklearn.neural_network import MLPRegressor
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import normalize

N_FEATURES = 12
N_LABELS = 4

# Prepare the data
data = pd.read_csv('data/walk00_vectors_and_mags.csv').values
X, y = data[:, :N_FEATURES], data[:, N_FEATURES:]

# Split train and test
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33)

# Build the model
model = MLPRegressor(
    hidden_layer_sizes=(32, 42,),
    activation='relu',
    solver='adam',
)

# Fit the model
model.fit(X_train, y_train)

# Evaluate the model
print(f'R^2={dutils.score_avg_rquared(model, X_test, y_test)}')
print(f'MSE={dutils.score_avg_mse(model, X_test, y_test)}')
