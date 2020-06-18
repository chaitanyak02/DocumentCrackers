
from keras.preprocessing import image
from keras.applications.vgg16 import VGG16
from keras.applications.vgg16 import preprocess_input
import numpy as np
from sklearn.cluster import KMeans
import os, shutil, glob, os.path
from PIL import Image as pil_image
image.LOAD_TRUNCATED_IMAGES = True 
model = VGG16(weights='imagenet', include_top=False)
import UploadFile as uploadFile
import traceback2 as traceback




def getListOfFiles(dirName):
    # create a list of file and sub directories 
    # names in the given directory 
    print('getListOfFiles......'+dirName)
    listOfFile = os.listdir(dirName)
    allFiles = list()
    # Iterate over all the entries
    for entry in listOfFile:
        # Create full path
        fullPath = os.path.join(dirName, entry)
        # If entry is a directory then get the list of files in this directory 
        if os.path.isdir(fullPath):
            allFiles = allFiles + getListOfFiles(fullPath)
        else:
            allFiles.append(fullPath)
        print('Len allFiles'+str(len(allFiles)))        
    return allFiles        
def UnsupervisedClassification(DiskPath,filelist):
    # Variables
    #imdir = r'C:\CK\welldata-images\Disk1'
    targetdir = os.getcwd()+"/"+  "Cluster/"+dirName
    number_clusters = 4
    featurelist = []
    for i, imagepath in enumerate(filelist):
        print("    Status: %s / %s" %(i, len(filelist)), end="\r")
        img = image.load_img(imagepath,target_size=(224, 224))
        img_data = image.img_to_array(img)
        img_data = np.expand_dims(img_data, axis=0)
        img_data = preprocess_input(img_data)
        features = np.array(model.predict(img_data))
        #features_flat = features.reshape(( -1,2))
        featurelist.append(features.flatten())

    # Clustering
    kmeans = KMeans(n_clusters=number_clusters, random_state=0).fit(np.array(featurelist))

    print('------------------------K-Means Clustering Completed------------------------')
    try:
        os.makedirs(targetdir)
    except OSError:
        pass
    #Copy  Image
    print("\n")
    for i, m in enumerate(kmeans.labels_):
        print("    Copy: %s / %s" %(i, len(kmeans.labels_)), end="\r")
        shutil.copy(filelist[i], targetdir + str(m) + "_" + str(i) + ".png") 
        #print('Before Upload Files')
        uploadFile.UploadFiles('converge-clusterimages',DiskPath,str(m) + "_" + str(i) + ".png",targetdir + str(m) + "_" + str(i) + ".png")

def ClassifyImages(DiskPath,imdir):
    try:
            # Get Files
            print('------------------------Image Classification Start------------------------')
            #print(' Classify images'+ imdir)
            filelist = getListOfFiles(imdir)
            #filelist = glob.glob(os.path.join(imdir, '*.png'))
            filelist.sort()
            UnsupervisedClassification(DiskPath,filelist)

    except Exception as e:
       print('Error occurred in Classification:::.', e)
       traceback.print_exc()


