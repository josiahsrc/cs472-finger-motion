import cv2
import numpy as np
import imutils


# @josiahsrc presets
# green:    min=np.array([46, 61, 97]), max=np.array([86, 255, 255])
# blue:     min=np.array([105, 96, 71]), max=np.array([125, 255, 144])
# purple:   min=np.array([135, 222, 30]), max=np.array([159, 255, 132])


#@edwardsrc presets
#pink (138, 89, 180), (162, 255, 255)
#purple (124, 147, 170), (132, 255, 255)
#blue (102, 150, 193), (113, 255, 255)
#green (54, 21, 168), (92, 255, 255)
#orange (2, 70, 230), (9, 255, 55)



class CameraCtrl():
    # basic camera controller. this simply initalizes the camera and passes on the video information
    def __init__(self):
        self.cap = cv2.VideoCapture(0)

        #These colors are HSV values
        #Edward presets:
        self.greenLower = np.array([34, 75, 32])
        self.greenUpper = np.array([84, 255, 242])

        self.orangeLower = np.array([2, 129, 197])
        self.orangeUpper = np.array([11, 255, 255])

        self.blueLower = np.array([102, 184, 89])
        self.blueUpper = np.array([112, 255, 255])

        self.purpleLower = np.array([128, 152, 80])
        self.purpleUpper = np.array([159, 255, 220])
        
        self.pinkLower = np.array([157, 139, 146])
        self.pinkUpper = np.array([176, 255, 255])

        
        # Josiah presets
        # self.greenLower = np.array([34, 75, 32])
        # self.greenUpper = np.array([84, 255, 242])

        # self.orangeLower = np.array([0, 210, 230])
        # self.orangeUpper = np.array([19, 255, 255])

        # self.blueLower = np.array([102, 184, 89])
        # self.blueUpper = np.array([112, 255, 255])

        # self.purpleLower = np.array([128, 152, 80])
        # self.purpleUpper = np.array([159, 255, 220])

        # self.pinkLower = np.array([157, 139, 146])
        # self.pinkUpper = np.array([176, 255, 255])

    def __del__(self):
        self.cap.release()
        cv2.destroyAllWindows()

    def createMask(self, img, lower, upper):
        mask = cv2.inRange(img, lower, upper)
        mask = cv2.erode(mask, None, iterations=1)
        mask = cv2.dilate(mask, None, iterations=1)
        return mask

    def bestContourCenter(self, mask):
        cnts = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        cnts = imutils.grab_contours(cnts)

        for cnt in cnts:
            ((x, y), radius) = cv2.minEnclosingCircle(cnt)
            M = cv2.moments(cnt)
            center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))

            if radius > 0.5 and radius < 100:
                return center

        return None

    #the green ones need to be handled a little differently. always orient left and right, assume never upside down.
    def bestGreenCenters(self, mask):
        cnts = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        cnts = imutils.grab_contours(cnts)

        centers = []
        for cnt in cnts:
            ((x, y), radius) = cv2.minEnclosingCircle(cnt)
            M = cv2.moments(cnt)
            center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))

            if radius > 3 and radius < 100:
                centers.append(center)
            
            if len(centers) >= 2:
                break
        
        center_l = None
        center_r = None
        if len(centers) >= 2:
            if centers[0] < centers[1]:
                center_l = centers[0]
                center_r = centers[1]
            else:
                center_l = centers[1]
                center_r = centers[0]

        return center_l, center_r


    def getFrame(self):
        success, frame = self.cap.read()

        if not success:
            print("no camera detected, closing....")
            exit()
        return frame

    def track(self, frame):
        blurred = cv2.GaussianBlur(frame, (11, 11), 0)
        hsv = cv2.cvtColor(blurred, cv2.COLOR_BGR2HSV)

        # gather each of the point colors

        # green is the origin we must handle it differently
        greenMask = self.createMask(hsv, self.greenLower, self.greenUpper)


        orangeMask = self.createMask(hsv, self.orangeLower, self.orangeUpper)
        blueMask = self.createMask(hsv, self.blueLower, self.blueUpper)
        purpleMask = self.createMask(hsv, self.purpleLower, self.purpleUpper)
        pinkMask = self.createMask(hsv, self.pinkLower, self.pinkUpper)

        # green is the root, connecting to blue and purple. blue connects to orange, and purple connects to pink
        masks = [blueMask, purpleMask, orangeMask, pinkMask]

        total_mask = greenMask | orangeMask | blueMask | purpleMask | pinkMask

        centers = np.full(6, None)

        green_l, green_r = self.bestGreenCenters(greenMask)
        centers[0] = green_l
        centers[1] = green_r

        for i in range(len(masks)):
            centers[i + 2] = self.bestContourCenter(masks[i])

        tracked_frame = np.copy(frame)
        for center in centers:
            if center is not None:
                cv2.circle(tracked_frame, center, 5, (0, 0, 255), -1)

        # draw connection between green and blue
        if centers[0] is not None and centers[1] is not None:
            cv2.line(tracked_frame, centers[0], centers[1], (255, 0, 255), 2)

        if centers[1] is not None and centers[2] is not None:
            cv2.line(tracked_frame, centers[1], centers[2], (0, 255, 0), 2)

        # draw connection between green and purple
        if centers[0] is not None and centers[3] is not None:
            cv2.line(tracked_frame, centers[0], centers[3], (0, 255, 0), 2)

        # draw connection between blue and orange
        if centers[2] is not None and centers[4] is not None:
            cv2.line(tracked_frame, centers[2], centers[4], (0, 255, 0), 2)

        # draw connection between purple and pink
        if centers[3] is not None and centers[5] is not None:
            cv2.line(tracked_frame, centers[3], centers[5], (0, 255, 0), 2)
        
        # return centers, blurred
        return centers, tracked_frame #total_mask
