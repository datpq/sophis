using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using sophis.log;

namespace MEDIO.RBCOrderAdapter
{
    public class CSxFTPManager
    {
        string _URL = RBCConfigurationSectionGroup.RBCSectionFTP.FtpUrl;
        int _PORT = RBCConfigurationSectionGroup.RBCSectionFTP.FtpPort;
        string _USERNAME = RBCConfigurationSectionGroup.RBCSectionFTP.FtpUsername;
        string _PASSWORD = RBCConfigurationSectionGroup.RBCSectionFTP.FtpPassword;
        string _TOFTP = RBCConfigurationSectionGroup.RBCFileSection.ToRBCFolder;
        string _EXTENSION = RBCConfigurationSectionGroup.RBCFileSection.OutFileExtension;

        public bool UploadToFTP(string fileName)
        {
            bool result = true;
            // Get the object used to communicate with the server.
            using (Logger log = new Logger(this, "UploadToFTP"))
            {
                log.log(Severity.debug, "Start");
                string _fileName = "";
                try
                {
                    _fileName = string.Format("{0}.{1}", fileName, _EXTENSION);
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", _URL, _fileName));
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    request.Credentials = new NetworkCredential(_USERNAME, _PASSWORD);
                    using (StreamReader sourceStream = new StreamReader(_TOFTP + "\\" + _fileName))
                    {
                        byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                        sourceStream.Close();

                        request.ContentLength = fileContents.Length;
                        Stream requestStream = request.GetRequestStream();
                        requestStream.Write(fileContents, 0, fileContents.Length);
                        requestStream.Close();

                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        response.Close();

                        log.log(Severity.info, string.Format("File uploaded to FTP, status: " + response.StatusDescription));
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    log.log(Severity.error, string.Format("Unable to upload file: " + _fileName + " to FTP"));
                    log.log(Severity.error, string.Format("Exception: " + ex));
                    log.end();
                }
                return result;
            }
        }
    }
}
