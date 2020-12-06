import socket
import time
import pandas
import sys
import cameractrl
import cv2
import numpy as np
import json
import csv
import os.path
import setup
import tensorflow as tf
import pandas as pd
import data_utils as dutils
import databuilder_bighefty2 as bighefty2

from multiprocessing import Process, Queue
from sklearn.model_selection import train_test_split
from sklearn.neural_network import MLPRegressor
from sklearn.preprocessing import normalize, Normalizer
from sklearn.experimental import enable_iterative_imputer
from sklearn.impute import IterativeImputer


# This is basically temporary class to get this project
# working. It houses various variables needed to do predictions
class DevContext:
    def __init__(self):
        self.model = None
        self.n_features = None
        self.n_labels = None
        self.imputer = None
        self.normalizer = None
        pass


# APPEND INSTANCE USING RAW POINTS
def appendInstance(reqTuple):
    # appends to a certian training data file
    request = reqTuple[0]
    points = reqTuple[2]

    filepath = request['csvPath']
    angles = np.copy(request['outputs'])

    # calculate points
    raw_points = []
    for point in points:
        if point is None:
            raw_points.append(None)
            raw_points.append(None)
        else:
            raw_points.append(point[0])
            raw_points.append(point[1])

    # Combining into one row
    final_row = np.concatenate((raw_points, angles), axis=0)

    # Make sure the specified folder exists
    dir_name = os.path.dirname(filepath)
    if not os.path.exists(dir_name):
        os.makedirs(dir_name)

    with open(filepath, 'a+') as fd:
        writer = csv.writer(fd)
        writer.writerow(final_row)

    # return the response
    return {'type': 'append_instance',
            'message': 'instance appended to {}'.format(filepath)}


def saveModel(reqTuple):
    # saves a pre-trained network
    pass


def loadModel():
    # loads a pre-trained network
    pass


def predict(reqTuple, context: DevContext):
    curr_points = reqTuple[2]
    prev_points = reqTuple[3]

    if prev_points is None or curr_points is None:
        return
    
    def get_val(tup, idx):
        return None if tup is None else tup[idx]
    
    curr_points = np.array([[
        get_val(curr_points[0], 0), get_val(curr_points[0], 1),
        get_val(curr_points[1], 0), get_val(curr_points[1], 1),
        get_val(curr_points[2], 0), get_val(curr_points[2], 1),
        get_val(curr_points[3], 0), get_val(curr_points[3], 1),
        get_val(curr_points[4], 0), get_val(curr_points[4], 1),
        get_val(curr_points[5], 0), get_val(curr_points[5], 1),
    ]])
    
    prev_points = np.array([[
        get_val(prev_points[0], 0), get_val(prev_points[0], 1),
        get_val(prev_points[1], 0), get_val(prev_points[1], 1),
        get_val(prev_points[2], 0), get_val(prev_points[2], 1),
        get_val(prev_points[3], 0), get_val(prev_points[3], 1),
        get_val(prev_points[4], 0), get_val(prev_points[4], 1),
        get_val(prev_points[5], 0), get_val(prev_points[5], 1),
    ]])
    
    # Impute missing values an normalize
    curr_points, prev_points = context.imputer.transform(curr_points), context.imputer.transform(prev_points)
    curr_points, prev_points = context.normalizer.transform(curr_points), context.imputer.transform(prev_points)
    
    # Make sure points are 1D
    curr_points, prev_points = curr_points.reshape(-1), prev_points.reshape(-1)
    
    # Make a prediction
    features = bighefty2.build_features(curr_points, prev_points).reshape(1, -1)
    prediction = context.model.predict(features).reshape(-1).tolist()
    
    # Return the prediction
    return {
        'type': 'predict',
        'prediction': prediction,
    }


def fit(reqTuple, context):
    pass


def score():
    # scores the model
    pass


def handleRequests(reqQ, respQ):
    ##############################
    ## REGION: CONTEXT SETUP    ##
    ##############################
    print('Setting up neural model...')

    # This stuff just exists to get the project working
    data = bighefty2.get_raw_input_data()
    N_LABELS = 4
    N_FEATURES = data.shape[1] - N_LABELS

    context = DevContext()
    context.n_features = N_FEATURES
    context.n_labels = N_LABELS

    # Build the data imputer
    print('Building value imputer...')
    context.imputer = IterativeImputer(max_iter=10).fit(data[:, :N_FEATURES])
    temp_imputed_data = context.imputer.transform(data[:, :N_FEATURES])
    
    # Build the data normalizer
    print('Building value normalizer...')
    context.normalizer = Normalizer().fit(temp_imputed_data)

    # Load the model
    print('Loading saved model...')
    context.model = tf.keras.models.load_model('saved/bighefty2')

    print('Neural model setup done!')
    ##################################
    ## END_REGION: CONTEXT SETUP    ##
    ##################################

    # Request thread handler
    print('Handle request loop started...')
    while True:
        reqTuple = reqQ.get()
        requestType = reqTuple[0]['type']
        resp = ()

        if requestType == 'append_instance':
            resp = appendInstance(reqTuple)
        elif requestType == 'save_model':
            resp = saveModel(reqTuple)
        elif requestType == 'load_model':
            resp = loadModel(reqTuple)
        elif requestType == 'fit':
            resp = fit(reqTuple, context.model)
        elif requestType == 'predict':
            resp = predict(reqTuple, context)
        elif requestType == 'score':
            resp = score(reqTuple)
        elif requestType == 'end':
            os.system('kill %d' % os.getpid())

        respQ.put(resp)


def handleResponses(respQ):
    print('Handle response loop started...')

    HOST, PORT = "127.0.0.1", 5065
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # Response thread handler
    while True:
        # Blocks until an item is available
        resp = respQ.get()

        if resp is None:
            continue

        resp_json = json.dumps(resp)
        resp_bytes = bytes(resp_json, "utf-8")
        nbytes = sock.sendto(resp_bytes, (HOST, PORT))
        print(f'Sent [{nbytes}/{len(resp_bytes)}] bytes back to client, with json={resp_json}.')


def main():
    print('Starting up server...')
    HOST, PORT, BUF_SIZE = "127.0.0.1", 5002, 4096
    reqQ = Queue(1024)
    respQ = Queue(1024)

    print('Starting processes...')
    RequestProcess = Process(target=handleRequests, args=(reqQ, respQ,))
    ResponseProcess = Process(target=handleResponses, args=(respQ,))
    RequestProcess.start()
    ResponseProcess.start()

    cam = cameractrl.CameraCtrl()

    # create a listening socket on the local host (reuse address)
    print('Binding to recv port...')
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    sock.bind((HOST, PORT))

    prevPoints = None
    prevRequest = None
    while True:
        request, addr = sock.recvfrom(BUF_SIZE)

        # decode the request and deserialize from json
        request = json.loads(request.decode())

        frame = cam.getFrame()
        points, trackedFrame = cam.track(frame)

        if (prevRequest is not None and prevPoints is not None):
            # combine previous request and current request and points into a tuple
            reqTuple = (request, prevRequest, points, prevPoints)
            reqQ.put(reqTuple)

        cv2.imshow("tracked_frame", trackedFrame)

        prevPoints = points
        prevRequest = request

        c = cv2.waitKey(1)
        if c == 27:
            break

    RequestProcess.join()
    ResponseProcess.join()

# '''


if __name__ == "__main__":
    main()
