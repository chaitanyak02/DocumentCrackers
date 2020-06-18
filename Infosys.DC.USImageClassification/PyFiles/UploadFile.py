from azure.storage.blob import BlockBlobService, PublicAccess
import os





def UploadFiles(ContainerName,FileLocation,file_name,path):
 try:
    #Uploading  the output file to Azure
   
    print('------------------------Uploading Files to '+ContainerName+'------------------------')
    block_blob_service_Managed = BlockBlobService(account_name='',
                                                    account_key='')
  
        
    block_blob_service_Managed.create_blob_from_path(ContainerName+"\\"+FileLocation, file_name, path)


    
    

 except Exception as e:
    print('Error occurred in uploadFilesToAzure.py for :::.'+file_name, e)

