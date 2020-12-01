import socket
import time
import sys
import cameractrl
import cv2
import numpy as np
from multiprocessing import Process, Queue
import json
import csv
import os.path


def appendInstance(reqTuple):
  #appends to a certian training data file
  request = reqTuple[0]
  prevRequest = reqTuple[1]
  points = reqTuple[2]
  prevPoints = reqTuple[3]

  #grab the filepath to write to,
  filepath = request['csvPath']

  #calculate delta angles
  angles = np.copy(request['outputs'])
  prev_angles = np.copy(prevRequest['outputs'])
  delta_angles = angles - prev_angles

  #calculate delta points
  delta_points = []
  for point1, point2 in zip(points, prevPoints):
    if point1 is None or point2 is None:
      delta_points.append(None)
    
    else:
      point1 = np.array(point1)
      point2 = np.array(point2)
      delta = point1 - point2
      delta_points.append(delta)

  # Clean up delta_points
  new_points = []
  for point in delta_points:
    if point is not None:
      new_points.append(point[0])
      new_points.append(point[1])
    else:
      new_points.append(None)
      new_points.append(None)

  # Combining into one row
  final_row = np.concatenate((new_points, delta_angles), axis=0)

  #now they need to be writen to the file
  # print('delta angles:', delta_angles)
  # print('delta positions:', delta_points)
  
  # Make sure the specified folder exists
  dir_name = os.path.dirname(filepath)
  if not os.path.exists(dir_name):
    os.makedirs(dir_name)

  with open(filepath, 'a+') as fd:
    writer = csv.writer(fd)
    writer.writerow(final_row)

  #return the response
  return {'type': 'append_instance',
          'message': 'instance appended to {}'.format(filepath)}

def saveModel(reqTuple):
  pass

def loadModel():
  #loads a pre-trained network
  pass

def predict():
  #will return angle delta predictions
  pass

def fit():
  #fits the data
  pass

def score():
  #scores the model
  pass

def handleRequests(reqQ, respQ):
  #Request thread handler
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
      resp = fit(reqTuple)
    elif requestType == 'predict':
      resp = predict(reqTuple)
    elif requestType == 'score':
      resp = score(reqTuple)
    elif requestType == 'end':
      break

    respQ.put(resp)


def handleResponses(respQ):
  print('Handle response loop started...')
  
  HOST, PORT = "127.0.0.1", 5065
  sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
  #Response thread handler
  while True:
    #Blocks until an item is available
    resp = respQ.get()
    
    if resp is None:
      continue
    
    resp_json = json.dumps(resp)
    resp_bytes = bytes(resp_json, "utf-8")
    nbytes = sock.sendto(resp_bytes, (HOST, PORT))
    print(f'Sent [{nbytes}/{len(resp_bytes)}] bytes back to client.')
  

def main():
  HOST, PORT, BUF_SIZE = "127.0.0.1", 5002, 4096
  reqQ = Queue(1024)
  respQ = Queue(1024)

  RequestProcess = Process(target=handleRequests, args=(reqQ,respQ,))
  ResponseProcess = Process(target=handleResponses, args=(respQ,))

  RequestProcess.start()
  ResponseProcess.start()

  cam = cameractrl.CameraCtrl()

  #create a listening socket on the local host (reuse address)
  sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
  sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
  sock.bind((HOST, PORT))
  
  print('Binded to recv port...')
  
  prevPoints = None
  prevRequest = None
  while True:
    request, addr = sock.recvfrom(BUF_SIZE)

    #decode the request and deserialize from json
    request = json.loads(request.decode())

    
    frame = cam.getFrame()
    points, trackedFrame = cam.track(frame)


    if (prevRequest is not None and prevPoints is not None):
      #combine previous request and current request and points into a tuple
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

#'''

if __name__ == "__main__":
  main()
