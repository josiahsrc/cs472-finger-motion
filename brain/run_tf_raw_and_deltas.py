import pandas as pd
import numpy as np
import tensorflow as tf
import data_utils as dutils
import setup

from sklearn.model_selection import train_test_split

# Report:

N_FEATURES = 20
N_LABELS = 4

# Prepare the data
data = pd.read_csv('data/walk00_raw_and_deltas.csv').values
X, y = dutils.prepare_data_imputed(data[:, :N_FEATURES], data[:, N_FEATURES:])

# Split train and test
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33)

# Build the model
model = tf.keras.models.Sequential([
    tf.keras.Input(shape=(N_FEATURES,), name="input"),
    tf.keras.layers.LayerNormalization(axis=1 , center=True , scale=True),
    tf.keras.layers.Dense(48, activation='relu', name='hidden_dense_1'),
    tf.keras.layers.Dense(64, activation='relu', name='hidden_dense_2'),
    tf.keras.layers.Dense(N_LABELS, activation='softmax', name='output')
])

# Compile the model
model.compile(
    loss='mean_squared_error',
    optimizer=tf.keras.optimizers.Adam(learning_rate=0.001)
)

# Fit the model
model.fit(X_train, y_train, epochs=6)

# Evaluate the model
score = model.evaluate(X_test, y_test)
print(f'MSE={score}')

# print(model.predict(X[1:3, :]).reshape(-1))
print(model.predict(X[1:3, :]).shape)
print(model.predict(X[1:3, :]))
