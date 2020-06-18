import ssl
ssl._create_default_https_context = ssl._create_unverified_context 
import os
import sys
#from azure.keyvault import KeyVaultClient, KeyVaultAuthentication
#from azure.common.credentials import ServicePrincipalCredentials
import json
import os, uuid, sys
import  AzureDownloadFile as down
from os import path
credentials = None
from azure.storage.blob import BlockBlobService, PublicAccess


'''This module  will download all keys from Keyvault and store in the JSON file  which is stored in the current working directory of the Batch VM'''
try:
        #Read the parameters from batch program 
        print('------------------------Starting Azure Batch------------------------')
        print ('Number of arguments:', len(sys.argv), ' arguments.')
        print ('Argument List:', str(sys.argv))
        print ('Current Working Directory' +os.getcwd())
        print('Args Loaded-START')
        
       
        
        print('Args Loaded-END')
   
        down.DownloadFiles(sys.argv[1])

except Exception as e:
       print('Error occurred in StartAzureBatch.py:::.', e)
       


