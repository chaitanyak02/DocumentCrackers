import cv2
import numpy as np
def sort_all_Imagecontours(cnts, method="left-to-right"):
    reverse = False
    i = 0
    if method == "right-to-left" or method == "bottom-to-top":
        reverse = True
    if method == "top-to-bottom" or method == "bottom-to-top":
        i = 1
   # Find  the list of Border boxes  from top to bottom
    bBoxes = [cv2.boundingRect(c) for c in cnts]
    (cnts, bBoxes) = zip(*sorted(zip(cnts, bBoxes),
                                        key=lambda b: b[1][i], reverse=reverse))

   
    return (cnts, bBoxes)

def Box_Detection(img_for_Box_Detection_path, cropped_dir_path):
        img = cv2.imread(img_for_Box_Detection_path, 0)  
        (thresh, img_bin) = cv2.threshold(img, 128, 255,
                                          cv2.THRESH_BINARY | cv2.THRESH_OTSU)  
        img_bin = 255-img_bin  
        cv2.imwrite("Image_Inverted.jpg",img_bin)
   
        # kernel length
        kernel_len = np.array(img).shape[1]//40
     
        # A verticle kernel to detect all the verticle lines from the image.
        vkernel = cv2.getStructuringElement(cv2.MORPH_RECT, (1, kernel_len))
        # A horizontal kernel to detect all the horizontal lines from the image.
        hkernel = cv2.getStructuringElement(cv2.MORPH_RECT, (kernel_len, 1))
        # Kernel
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3))
        # Morphological operation for detecting verticle lines 
        temp_Image = cv2.erode(img_bin, vkernel, iterations=3)
        verticle_lines_image = cv2.dilate(temp_Image, vkernel, iterations=3)
        cv2.imwrite("verticle_lines.jpg",verticle_lines_image)
        # Morphological operation for detecting horizontal lines
        img_temp2 = cv2.erode(img_bin, hkernel, iterations=3)
        horizontal_lines_image = cv2.dilate(img_temp2, hkernel, iterations=3)
        cv2.imwrite("horizontal_lines.jpg",horizontal_lines_image)
        # Weighting parameters
        alpha = 0.5
        beta = 1.0 - alpha

        Final_Bin_Image = cv2.addWeighted(verticle_lines_image, alpha, horizontal_lines_image, beta, 0.0)
        Final_Bin_Image = cv2.erode(~Final_Bin_Image, kernel, iterations=2)
        (thresh, Final_Bin_Image) = cv2.threshold(Final_Bin_Image, 128, 255, cv2.THRESH_BINARY | cv2.THRESH_OTSU)
    
        cv2.imwrite("Final_Bin_Image.jpg",Final_Bin_Image)
        # Find Imagecontours for the image for detecting all the boxes
        Imagecontours, hierarchy = cv2.findContours(
            Final_Bin_Image, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        # Sort all the Imagecontours by top to bottom.
        (Imagecontours, bBoxes) = sort_all_Imagecontours(Imagecontours, method="top-to-bottom")
        idx = 0
        for c in Imagecontours:
           
            x, y, w, h = cv2.boundingRect(c)

            if (w > 5 and h > 5)    :
                idx += 1
                new_img = img[y:y+h, x:x+w]
                cv2.imwrite(cropped_dir_path+str(idx) + '.png', new_img)
Box_Detection("C:\\Projects\\Data Mining RFS\\249\\249_I0_0.jpg", "C:\\Projects\\Data Mining RFS\\249\\Cropped\\")
