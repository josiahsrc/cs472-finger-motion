import warnings
import pandas as pd
import numpy as np
import data_utils as dutils
import setup

from sklearn.neural_network import MLPRegressor
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import normalize

N_FEATURES = 45
N_LABELS = 4

# Prepare the data
data = pd.read_csv('data/walk00_big_hefty.csv').values
X, y = data[:, :N_FEATURES], data[:, N_FEATURES:]

# Split train and test
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33)

# Build the model
model = MLPRegressor(
    hidden_layer_sizes=(100, 100, 40),
    activation='relu',
    solver='adam',
)

# Fit the model
model.fit(X_train, y_train)

# Evaluate the model
score = dutils.score_avg_mse(model, X_test, y_test)
print(f'MSE={score}')
