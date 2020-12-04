import warnings
import pandas as pd
import numpy as np
import data_utils as dutils
import setup

from sklearn.neural_network import MLPRegressor
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import normalize

# Report:
# This model produces inconsistent results. 
#
# Some runs produced these MSEs:
# MSE=0.016371870558437668
# MSE=0.016371870558437668
# MSE=0.2760732442505571
#
# There isn't really a pattern most likely because the
# points of the character can be anywhere in the domain
# of the screen (its hard to keep your hand in the same
# spot when training).

N_FEATURES = 20
N_LABELS = 4

# Prepare the data
data = pd.read_csv('data/walk00_raw_and_deltas.csv').values
X, y = dutils.prepare_data_imputed_norm(data[:, :N_FEATURES], data[:, N_FEATURES:])

# Split train and test
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33)

# Build the model
model = MLPRegressor(
    hidden_layer_sizes=(40, 80, 80, 40, 20),
    activation='relu',
    solver='adam',
)

# Fit the model
model.fit(X_train, y_train)

# Evaluate the model
print(f'R^2={dutils.score_avg_rquared(model, X_test, y_test)}')
print(f'MSE={dutils.score_avg_mse(model, X_test, y_test)}')
