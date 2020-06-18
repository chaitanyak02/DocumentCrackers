import os, uuid, sys
import PIL
# import CheckFileType as FileType
from azure.storage.blob import BlockBlobService, PublicAccess
import os.path
from os import path

from PIL import Image, TiffTags, ExifTags
from PIL.ExifTags import TAGS 
from PIL import PngImagePlugin
import ExtractmagesFromPDF as ImageExtract
import UnsupervisedImageClassification as classifcation
from pathlib import Path
import pathlib
import ImageProcessing as IP
import glob


def DownloadFiles(DiskPath):
    try:
   

                        

            # Create the BlockBlockService and Blob service for the storage account
            block_blob_service_Managed = BlockBlobService(account_name='',
                                                            account_key='')
            print('Connected to Managed Storage Container')
            print('DiskPath'+DiskPath)
            if not os.path.exists(os.getcwd()+'/'+DiskPath):
                os.makedirs(os.getcwd()+'/'+DiskPath)
            #FileName=os.path.splitext(os.path.basename(Constants.Download_File_Path))[0]+os.path.splitext(os.path.basename(Constants.Download_File_Path))[1]
             
            #FileFullPathLocation=os.path.dirname(FileFullPath)
            ContainerName='welldata'
            ContainerLocation=ContainerName
            print('ContainerLocation'+ContainerLocation)
            generator = block_blob_service_Managed.list_blobs(ContainerName, prefix=DiskPath)
            for blob in generator:
                print("\t Blob name: " + blob.name)
            #if (str(path.exists(FileFullPathLocation)) == False):
            #        print("\nDownloading blob to " + FileFullPathLocation)
                block_blob_service_Managed.get_blob_to_path(
                        ContainerLocation , blob.name,os.getcwd()+'/'+blob.name)
                print('Blob Download is Sucessful!!')
                # CHECK FILE TYPE
            

    
            for filename in os.listdir(os.getcwd()+'/'+DiskPath):
                if filename.endswith(".pdf") or filename.endswith(".PDF"):
                    fileName=os.path.splitext(filename)[0]
                    ImageExtract.ImageExtraction(fileName,DiskPath,os.getcwd()+'/'+DiskPath+'/'+filename)
             
            print('--------------------------------------Image Extraction Completed-------------------------------')        
            print('Disk'+os.getcwd()+'\\'+str(DiskPath))
            #classifcation.ClassifyImages(DiskPath,os.getcwd()+'\\'+str(DiskPath)+'\\'+str(dir)) 
            #for filename in os.listdir(Path(os.getcwd()+'\\'+str(DiskPath))):
            #dir_list = os.walk('.').next()[1]
            print('--------------------------------------Image Processing Start-------------------------------')  
            #import glob

            files = glob.glob(os.getcwd()+'\\'+str(DiskPath) + '/**/*.jpg', recursive=True)
            for fileName in files:
                IP.StartImageProcessing(fileName,DiskPath)

            #IP.StartImageProcessing()
            scanDir=os.getcwd()+'\\'+str(DiskPath)
            directories = [d for d in os.listdir(scanDir) if os.path.isdir(os.path.join(os.path.abspath(scanDir), d))]
            print(directories)
            
            for dir in directories:
                    classifcation.ClassifyImages(DiskPath,os.getcwd()+'\\'+str(DiskPath)+'\\'+str(dir)+'\\'+'Enhanced')        

    except Exception as e:
        print('Error occurred in DownloadFilesFromBlob:::.', e)

    finally:
        print('\nAzure Storage Blob sample - Completed.\n')





