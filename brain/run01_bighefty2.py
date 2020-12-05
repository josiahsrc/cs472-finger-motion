import warnings
import pandas as pd
import numpy as np
import tensorflow as tf
import data_utils as dutils
import setup

from sklearn.neural_network import MLPRegressor
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import normalize

# Report:

data = pd.read_csv('data/bighefty2.csv').values

N_LABELS = 4
N_FEATURES = data.shape[1] - N_LABELS

# Prepare the data
X, y = dutils.prepare_data_imputed_norm(data[:, :N_FEATURES], data[:, N_FEATURES:])

# Split train and test
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33)

# # Build the model
# model = MLPRegressor(
#     hidden_layer_sizes=(150, 200, 100,),
#     activation='relu',
#     solver='adam',
# )

# # Fit the model
# model.fit(X_train, y_train)

# # Evaluate the model
# print(f'R^2={dutils.score_avg_rquared(model, X_test, y_test)}')
# print(f'MSE={dutils.score_avg_mse(model, X_test, y_test)}')

# Build the model
model = tf.keras.models.Sequential([
    tf.keras.Input(shape=(N_FEATURES,), name="input"),
    tf.keras.layers.LayerNormalization(axis=1 , center=True , scale=True),
    tf.keras.layers.Dense(150, activation='relu'),
    tf.keras.layers.Dense(200, activation='sigmoid'),
    tf.keras.layers.Dense(100, activation='relu'),
    tf.keras.layers.Dense(180, activation='elu'),
    tf.keras.layers.Dense(190, activation='relu'),
    tf.keras.layers.Dense(150, activation='relu'),
    tf.keras.layers.Dense(100, activation='swish'),
    tf.keras.layers.Dense(N_LABELS, activation='relu', name='output')
])

# Compile the model
model.compile(
    loss='mean_squared_error',
    optimizer=tf.keras.optimizers.Adam(learning_rate=0.001)
)

# Fit the model
model.fit(X_train, y_train, epochs=500)

# Evaluate the model
score = model.evaluate(X_test, y_test)
print(f'MSE={score}')
