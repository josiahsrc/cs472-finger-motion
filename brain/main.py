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
    # The issue here is that the inputs ARENT normalized

    curr_points = reqTuple[2]
    prev_points = reqTuple[3]

    if prev_points is None or curr_points is None:
        return

    prev_pt_grn = None if prev_points[0] is None else np.array(prev_points[0])
    prev_pt_blu = None if prev_points[1] is None else np.array(prev_points[1])
    prev_pt_prp = None if prev_points[2] is None else np.array(prev_points[2])
    prev_pt_org = None if prev_points[3] is None else np.array(prev_points[3])
    prev_pt_pnk = None if prev_points[4] is None else np.array(prev_points[4])

    pt_grn = None if curr_points[0] is None else np.array(curr_points[0])
    pt_blu = None if curr_points[1] is None else np.array(curr_points[1])
    pt_prp = None if curr_points[2] is None else np.array(curr_points[2])
    pt_org = None if curr_points[3] is None else np.array(curr_points[3])
    pt_pnk = None if curr_points[4] is None else np.array(curr_points[4])

    # Directional finger distance vectors
    diff_grn = None if pt_grn is None or prev_pt_grn is None else pt_grn - prev_pt_grn
    diff_blu = None if pt_blu is None or prev_pt_blu is None else pt_blu - prev_pt_blu
    diff_prp = None if pt_prp is None or prev_pt_prp is None else pt_prp - prev_pt_prp
    diff_org = None if pt_org is None or prev_pt_org is None else pt_org - prev_pt_org
    diff_pnk = None if pt_pnk is None or prev_pt_pnk is None else pt_pnk - prev_pt_pnk

    # Input into the network
    input = np.array([
        # Deltas
        None if diff_grn is None else diff_grn[0],
        None if diff_grn is None else diff_grn[1],

        None if diff_blu is None else diff_blu[0],
        None if diff_blu is None else diff_blu[1],

        None if diff_prp is None else diff_prp[0],
        None if diff_prp is None else diff_prp[1],

        None if diff_org is None else diff_org[0],
        None if diff_org is None else diff_org[1],

        None if diff_pnk is None else diff_pnk[0],
        None if diff_pnk is None else diff_pnk[1],

        # Points
        None if pt_grn is None else pt_grn[0] - pt_grn[0],
        None if pt_grn is None else pt_grn[1] - pt_grn[1],

        None if pt_blu is None else pt_blu[0] - pt_grn[0],
        None if pt_blu is None else pt_blu[1] - pt_grn[1],

        None if pt_prp is None else pt_prp[0] - pt_grn[0],
        None if pt_prp is None else pt_prp[1] - pt_grn[1],

        None if pt_org is None else pt_org[0] - pt_grn[0],
        None if pt_org is None else pt_org[1] - pt_grn[1],

        None if pt_pnk is None else pt_pnk[0] - pt_grn[0],
        None if pt_pnk is None else pt_pnk[1] - pt_grn[1],
    ]).reshape(1, -1)

    # Impute missing values
    input = context.imputer.transform(input)
    
    # Normalize the data based on past findings
    input = context.normalizer.transform(input)

    # Predict output
    output = context.model.predict(input).reshape(-1).tolist()

    # Return the prediction
    return {'type': 'predict',
            'prediction': output}


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
    data = pd.read_csv('data/walk00_raw_and_deltas.csv').values
    N_FEATURES = 20
    N_LABELS = 4

    context = DevContext()
    context.n_features = N_FEATURES
    context.n_labels = N_LABELS

    # Build the data imputer
    context.imputer = IterativeImputer(max_iter=10).fit(data[:, :N_FEATURES])
    
    # Build the data normalizer
    context.normalizer = Normalizer().fit(data[:, :N_FEATURES])

    # Prepare the data
    X, y = dutils.prepare_data_imputed(data[:, :N_FEATURES], data[:, N_FEATURES:])
    X_train, y_train = X, y

    # Build the model
    context.model = MLPRegressor(
        hidden_layer_sizes=(32, 42,),
        activation='relu',
        solver='adam',
    )

    # Fit the model
    print('Fitting neural model...')
    context.model.fit(X_train, y_train)

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
    HOST, PORT, BUF_SIZE = "127.0.0.1", 5002, 4096
    reqQ = Queue(1024)
    respQ = Queue(1024)

    RequestProcess = Process(target=handleRequests, args=(reqQ, respQ,))
    ResponseProcess = Process(target=handleResponses, args=(respQ,))

    RequestProcess.start()
    ResponseProcess.start()

    cam = cameractrl.CameraCtrl()

    # create a listening socket on the local host (reuse address)
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    sock.bind((HOST, PORT))

    print('Binded to recv port...')

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
