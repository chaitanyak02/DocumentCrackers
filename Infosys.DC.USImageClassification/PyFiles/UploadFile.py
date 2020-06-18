from azure.storage.blob import BlockBlobService, PublicAccess
import os





def UploadFiles(ContainerName,FileLocation,file_name,path):
 try:
    #Uploading  the output file to Azure
   
    print('------------------------Uploading Files to '+ContainerName+'------------------------')
    block_blob_service_Managed = BlockBlobService(account_name='welldatastg',
                                                    account_key='BN19h+Zlfw4GqDozASJEUsir4dAf0n7lrIOhU708uw6eBXnoiSw0JoWF51x/cAOlEzSVGrk4Yn4L9T0l88FyGw==')
  
        
    block_blob_service_Managed.create_blob_from_path(ContainerName+"\\"+FileLocation, file_name, path)


    
    

 except Exception as e:
    print('Error occurred in uploadFilesToAzure.py for :::.'+file_name, e)

