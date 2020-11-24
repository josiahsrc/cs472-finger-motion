import cv2
import numpy as np
import imutils

class CameraCtrl():
#basic camera controller. this simply initalizes the camera and passes on the video information
    def __init__(self):
        self.cap = cv2.VideoCapture(0)

        self.greenUpper = np.array([83, 255, 255])
        self.greenLower = np.array([59, 38, 171])

        self.orangeUpper = np.array([10,255,255])
        self.orangeLower = np.array([0,101,177])


        self.blueUpper = np.array([115,255,255])
        self.blueLower = np.array([107,174,138])

        self.purpleUpper = np.array([140,255,213])
        self.purpleLower = np.array([123,128,109])

        self.pinkUpper = np.array([179,255,255])
        self.pinkLower = np.array([148,92,107])

    def __del__(self):
        self.cap.release()
        cv2.destroyAllWindows()


    def createMask(self, img, lower, upper):
        mask = cv2.inRange(img, lower, upper)
        mask = cv2.erode(mask, None, iterations=1)
        mask = cv2.dilate(mask, None, iterations=1)
        return mask

    def bestContourCenter(self, mask):
        cnts = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL,
        cv2.CHAIN_APPROX_SIMPLE)
        cnts = imutils.grab_contours(cnts)

        for cnt in cnts:
            ((x,y), radius) = cv2.minEnclosingCircle(cnt)
            M = cv2.moments(cnt)
            center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))

            if radius > 1.5 and radius < 20:
                return center
                
        return None



    def getFrame(self):
        success, frame = self.cap.read()

        if not success:
            print("no camera detected, closing....")
            exit()
        return frame


    def track(self, frame):

        blurred = cv2.GaussianBlur(frame, (11, 11), 0)
        hsv = cv2.cvtColor(blurred, cv2.COLOR_BGR2HSV)

        #gather each of the point colors
        greenMask = self.createMask(hsv, self.greenLower, self.greenUpper)
        orangeMask = self.createMask(hsv, self.orangeLower, self.orangeUpper)
        blueMask = self.createMask(hsv, self.blueLower, self.blueUpper)
        purpleMask = self.createMask(hsv, self.purpleLower, self.purpleUpper)
        pinkMask = self.createMask(hsv, self.pinkLower, self.pinkUpper)
        
        #green is the root, connecting to blue and purple. blue connects to orange, and purple connects to pink
        masks = [greenMask, blueMask, purpleMask, orangeMask, pinkMask]

        total_mask = greenMask | orangeMask | blueMask | purpleMask | pinkMask

        centers = np.full(5, None)

        for i in range(len(masks)):
            centers[i] = self.bestContourCenter(masks[i])

        tracked_frame = np.copy(frame)
        for center in centers:
            if center is not None:
                cv2.circle(tracked_frame, center, 5, (0, 0, 255), -1)

        #draw connection between green and blue
        if centers[0] is not None and centers[1] is not None:
            cv2.line(tracked_frame, centers[0], centers[1], (0,255,0), 2)

        #draw connection between green and purple
        if centers[0] is not None and centers[2] is not None:
            cv2.line(tracked_frame, centers[0], centers[2], (0,255,0), 2)

        #draw connection between blue and orange
        if centers[1] is not None and centers[3] is not None:
            cv2.line(tracked_frame, centers[1], centers[3], (0,255,0), 2)
            
        #draw connection between purple and pink
        if centers[2] is not None and centers[4] is not None:
            cv2.line(tracked_frame, centers[2], centers[4], (0,255,0), 2)

        return centers, tracked_frame