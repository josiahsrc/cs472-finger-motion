import time
import pandas
import sys
import cameractrl
import cv2
import numpy as np

cam = cameractrl.CameraCtrl()

while True:
  
  frame = cam.getFrame()
  points, trackedFrame = cam.track(frame)
  
  cv2.imshow("tracked_frame", trackedFrame)
  
  c = cv2.waitKey(1)
  if c == 27:
    break
