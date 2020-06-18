import tempfile
import os
import cv2
import numpy as np
from PIL import Image,ImageFilter
Image.MAX_IMAGE_PIXELS = 10000000000    
from azure.storage.blob import BlockBlobService, PublicAccess
IMAGE_SIZE = 1800
BINARY_THREHOLD = 60
import UploadFile as uploadFile

#import RetrieveValuesFromTableStorage as retrieveTableStorage
#import Constants
#import InsertintoCosmosDB as insertintoCosmos



def StartImageProcessing(filepath,DiskPath):
    try:
       EnhancedImagePath=ImageProcess(filepath,DiskPath)

       return EnhancedImagePath
    except Exception as e:
       print('Error occurred in process_image_for_ocr definition:::.', e)


def process_image_for_ocr(file_path):
    try:
        print('in process_image_for_ocr')
        # TODO : Implement using opencv
        temp_filename = set_image_dpi(file_path)
        im_new = remove_noise_and_smooth(temp_filename)
        #print('Done')
        return im_new
    except Exception as e:
       print('Error occurred in process_image_for_ocr definition:::.', e)

def set_image_dpi(file_path):
    try:
        #print('in set_image_dpi')
        im = Image.open(file_path).convert('L')
        #im = Image.open(file_path)
        im=im.filter(ImageFilter.SHARPEN)
        #im = cv2.cvtColor( im, cv2.COLOR_RGB2GRAY )


        length_x, width_y = im.size
        factor = max(1, int(IMAGE_SIZE / length_x))
        size = factor * length_x, factor * width_y
        # size = (1800, 1800)
        im_resized = im.resize(size, Image.ANTIALIAS)
        temp_file = tempfile.NamedTemporaryFile(delete=False, suffix='.jpg')
        temp_filename = temp_file.name
        im_resized.save(temp_filename, dpi=(500, 500))
        return temp_filename
    except Exception as e:
       print('Error occurred in set_image_dpi definition:::.', e)


def image_smoothening(img):
    try:
        #print('3')
        ret1, th1 = cv2.threshold(img, BINARY_THREHOLD, 255, cv2.THRESH_BINARY)
        ret2, th2 = cv2.threshold(th1, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        blur = cv2.GaussianBlur(th2, (1, 1), 0)
        ret3, th3 = cv2.threshold(blur, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
        return th3

    except Exception as e:
       print('Error occurred in image_smoothening definition:::.', e)

def remove_noise_and_smooth(file_name):
    try:
        #print('in remove_noise_and_smooth')
        img = cv2.imread(file_name, 0)
        filtered = cv2.adaptiveThreshold(img.astype(np.uint8), 255, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY, 11, 2)
        kernel = np.ones((1, 1), np.uint8)
        opening = cv2.morphologyEx(filtered, cv2.MORPH_OPEN, kernel)
        closing = cv2.morphologyEx(opening, cv2.MORPH_CLOSE, kernel)
        img = image_smoothening(img)
        or_image = cv2.bitwise_or(img, closing)
        
        return or_image
    except Exception as e:
       print('Error occurred in remove_noise_and_smooth definition:::.', e)


def ImageProcess(path,DiskPath):
   try:

        if("Splits" in path):
                outputDir=path.rsplit('/', 1)[0]+"\\Splits-Enhanced\\"
                if (os.path.isdir(outputDir)==False) :
                    os.makedirs(outputDir, exist_ok=True)
                    print('Created Enhanced images directory for splits')
        else:
                print(path)
                outputDir=path.rsplit('\\', 1)[0]+"\\Enhanced\\"
                if (os.path.isdir(outputDir)==False) :
                    os.makedirs(outputDir, exist_ok=True)
                    print('Created Enhanced images directory')
    
      
                file_name=path.rsplit('/', 1)[-1]
                #print (os.path.join(root,filename) )
             
                #file_name_withoutext=Constants.FileName
                file_name_withoutext=path.rsplit('\\', 1)[1].split('.')[0]

                im_new=process_image_for_ocr(path)
                kernel = np.ones((1,1),np.uint8)
                closing = cv2.morphologyEx(im_new, cv2.MORPH_CLOSE, kernel)
                #kernel = np.ones((1,1),np.uint8)
                #erosion = cv2.erode(im_new,kernel,iterations = 50)

                EnhancedImagePath=os.path.join(outputDir,file_name_withoutext+'_Enhanced.png')
                cv2.imwrite(EnhancedImagePath,closing)
                #cv2.waitKey(0)
             
                #fetch ContainerName From StorageContainer For ProcessedImages
                #StorageAcountName=retrieveTableStorage.FetchValueFromTableStorage('ProcessedStorageAccountNameWest')
                #import Constants as cons
                #OriginLoc=cons.OriginLocation
                #if(OriginLoc=='WH'):
                    #ProcessedSContainerName=retrieveTableStorage.FetchValueFromTableStorage('ProcessedStorageAccountNameWest')
                #if(OriginLoc=='EH'):
                    #ProcessedSContainerName=retrieveTableStorage.FetchValueFromTableStorage('ProcessedStorageAccountNameEast')

                #Comment this
                #ProcessedSContainerName=Constants.ProcessedSContainerName
                #StorageAcountName=retrieveTableStorage.FetchValueFromTableStorage('StorageAccountNameEast')
                
                #uploadFiles.UploadFiles(ProcessedSContainerName,file_name_withoutext+'_Enhanced.png',os.path.join(outputDir,file_name_withoutext+'_Enhanced.png'))
                uploadFile.UploadFiles('converge-enhancedimages',DiskPath,file_name_withoutext+'_Enhanced.png',EnhancedImagePath)
                print('----------------Image Processing Completed for '+file_name_withoutext+'----------------')
                #print('upload Done!')

                #import Constants as cons
                #OriginLoc=cons.OriginLocation
                
                #UnComment Production
                #FETCH STOAREGE ACCOUNT NAME
                #if(OriginLoc=='WH'):
                    #StorageAcountName=retrieveTableStorage.FetchValueFromTableStorage('StorageAccountNameWest')
                #if(OriginLoc=='EH'):
                    #StorageAcountName=retrieveTableStorage.FetchValueFromTableStorage('StorageAccountNameEast')
                #UnComment Production
                #StorageAcountName='029ze1b1storslbpcom'
               # print('upload Done!!!')
                #print('path:::'+path)
                #path=path.replace("/", "\\")
                #absPath=path.rsplit('\\', 1)[0]
                #cwd=os.getcwd()
                #fullpath="".join(absPath.rsplit(cwd))
                #fullpath=fullpath.lstrip("\\")
                #print('fullpath:::'+fullpath)
                #fullpath=fullpath.replace("\\", "/")
                #print('After fullpath:::'+fullpath)
                #StorageAcountName = retrieveTableStorage.FetchValueFromTableStorage(Constants.UploadStorageAccountName)
                #StorageAcountName='stgactssabackupdev'
                #blobpath='https://'+StorageAcountName+'.blob.core.windows.net/'+ ProcessedSContainerName+'/'+fullpath+'/'+file_name_withoutext+'_Enhanced.png'
                
                #insertintoCosmos.updateDocumentsinCosmos(file_name_withoutext+'.TIF','EnhancedImage_FilePath',blobpath)

                return EnhancedImagePath

   except Exception as e:
         print('Error occurred in ImageProcess :::.', e)
         
if __name__=='__main__':
    StartImageProcessing(r'C:\\CK\\Converge2020\\welldata-images\\Disk1\\237\\p0-7.png')
