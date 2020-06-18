import ssl
import os
import sys
import json
import os, uuid, sys
from os import path
from applicationinsights import TelemetryClient
from azure.storage.blob import BlockBlobService, PublicAccess
import socket
import datetime

#Used for Testing Puurpose
def TestException():
    try:
        TrackEvent('RF056047967_00173521_WDF0410688_Small.tif','ProcessTiff-Start',datetime.datetime.now(),None )
        TrackEvent('RF056047967_00173521_WDF0410688_Small.tif','ProcessTiff-Start',datetime.datetime(2017, 6, 21, 18, 25, 30),datetime.datetime(2017, 5, 16, 8, 21, 10))
        startTime = datetime.datetime.now()
        raise Exception("Bhoom!!!")
    except Exception as e:
        #tc = TelemetryClient(Constants.AppInsight_Key)
        #tc.track_exception()
        #tc.flush()
        TrackException(e, 'RF056047967_00173521_WDF0410688_Small.tif','ProcessTiff-Start',startTime,datetime.datetime(2017, 5, 16, 8, 21, 10))

#Returns Datet time difference in minutes
def TimeDiffMinutes(start,end):
    if end is None:
        return 0
    else:
        return (end - start).seconds / 60

#Logs Event with spcified parameters in Appinsight    
def TrackEvent(File, Event, startTime,endTime):
    
    try:

        tc = TelemetryClient('')
        host_name = socket.gethostname() 
        host_ip = socket.gethostbyname(host_name) 
        tc.track_event(Event, { 'Server': host_name , 'IP': host_ip ,'Event':Event,'File':File}, { 'TimeTaken': TimeDiffMinutes(startTime,endTime)})
        tc.flush()
       
    except:
        tc.track_exception()
#Logs Exception with spcified parameters in Appinsight
def TrackException(exp,File, Event, startTime,endTime):
    try:
        raise exp
    except:
        tc = TelemetryClient(Constants.AppInsight_Key)
        host_name = socket.gethostname() 
        host_ip = socket.gethostbyname(host_name) 
        tc.track_exception(*sys.exc_info(), { 'Server': host_name , 'IP': host_ip ,'Event':Event,'File':File}, { 'TimeTaken': TimeDiffMinutes(startTime,endTime) })
        tc.flush()

# Used for Debugging Purpose. Uncomment during debug and comment after fixing.
#if __name__ == '__main__': TestException()