import socket
import time
import sys
import cameractrl
import cv2
import numpy as np
from multiprocessing import Process, Queue
import json


def appendInstance(reqTuple):
  #appends to a certian training data file
  request = reqTuple[0]
  prevRequest = reqTuple[1]
  points = reqTuple[2]
  prevPoints = reqTuple[3]

  #grab the filepath to write to,
  filepath = request.csvPath

  #calculate delta angles
  angles = np.copy(request.outputs)
  prev_angles = np.copy(prevRequest.outputs)
  delta_angles = angles - prev_angles

  #calculate delta points
  delta_points = []
  for point1, point2 in zip(points, prevPoints):
    if point1 is None or point2 is None:
      delta_points.append(None)
    
    else:
      delta = point1 - point2
      delta_points.append(delta)


  #now they need to be writen to the file
  print('delta angles:', delta_angles)
  print('delta positions:', delta_points)

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
  
  while True:
    reqTuple = reqQ.get()
    requestType = reqTuple[0].type
    resp = ()

    if requestType == 'append_instance':
      rest = appendInstance(reqTuple)
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
  #Response thread handler
  while True:
    resp = respQ.get()
    
  

def main():
  HOST, PORT = "127.0.0.1", 5002
  reqQ = Queue(1024)
  respQ = Queue(1024)


  RequestProcess = Process(target=handleRequests, args=(reqQ,respQ,))
  ResponseProcess = Process(target=handleResponses, args=(respQ,))

  RequestProcess.start()
  ResponseProcess.start()

  cam = cameractrl.CameraCtrl()


  #create a listening socket
  sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
  #use local host
  address = (HOST, PORT)
  sock.bind(address)
  


  prevPoints = None
  prevRequest = None
  while True:
    request, addr = sock.recvfrom(1024)

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
