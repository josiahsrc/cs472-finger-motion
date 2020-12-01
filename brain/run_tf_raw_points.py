import sklearn.model_selection as sk
import pandas as pd
import numpy as np
import tensorflow as tf

from tensorflow.keras.layers.experimental import preprocessing

# Report:
# This model produces horrible results. MSE=30188.2734375
# This is because the it's just using the raw points of the hand
# and correlating those to the virtual pants. This doesn't work
# because the hand moves around the screen alot, its not possible
# to keep it still while training, so the values are all wonky.

N_FEATURES = 10
N_LABELS = 4

# Prepare the data
data = pd.read_csv('data/walk00_raw.csv').values
X, y = data[:, :N_FEATURES], data[:, N_FEATURES:]
X, y = np.nan_to_num(X), y

# Split train and test
X_train, X_test, y_train, y_test = sk.train_test_split(X, y, test_size=0.33, random_state=42)

# Build the model
model = tf.keras.models.Sequential([
    tf.keras.Input(shape=(N_FEATURES,), name="input"),
    tf.keras.layers.LayerNormalization(axis=1 , center=True , scale=True),
    tf.keras.layers.Dense(15, activation='relu', name='hidden_dense_1'),
    tf.keras.layers.Dense(23, activation='relu', name='hidden_dense_2'),
    tf.keras.layers.Dense(18, activation='relu', name='hidden_dense_3'),
    tf.keras.layers.Dropout(0.2, name='hidden_dropout'),
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
