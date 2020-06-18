import sys,os
import PyPDF2
from PIL import Image

import fitz
import io
from pdfminer.converter import TextConverter
from pdfminer.pdfinterp import PDFPageInterpreter
from pdfminer.pdfinterp import PDFResourceManager
from pdfminer.pdfpage import PDFPage
from pdfminer.pdfinterp import resolve1
from pdfminer.pdfparser import PDFParser
from pdfminer.pdfdocument import PDFDocument
from pathlib import Path
import UploadFile as uploadFile
import UnsupervisedImageClassification as classifcation


def ImageExtraction(folderName,DiskPath,filepath):
    try:
         #print(fileName+'::::::fileName')
         FileLocation=os.path.dirname(filepath)+"\\"+Path(filepath).stem
         os.makedirs(FileLocation,exist_ok=True)
         doc = fitz.open(filepath)
         print('------------------------Extracting Images from '+filepath+'------------------------')
         for i in range(len(doc)):
            for img in doc.getPageImageList(i):
                xref = img[0]
                pix = fitz.Pixmap(doc, xref)
                imagepath=FileLocation+"\\%sp%s%s.jpg" % (folderName,i, xref)
                fileNamewithExt=os.path.basename(imagepath)
                #print('fileNamewithExt::::::::::'+fileNamewithExt)
                if pix.n < 5:       # this is GRAY or RGB
                    pix.writePNG(imagepath)
                else:               # CMYK: convert to RGB first
                    pix1 = fitz.Pixmap(fitz.csRGB, pix)
                    pix1.writePNG(imagepath)
                    pix1 = None
                pix = None
              
         

    except Exception as e:
       print('Error occurred in ImageExtraction:::.', e)
       #traceback.print_exc()