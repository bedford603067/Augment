using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Configuration;

namespace MultiPurposeService.Public.Http
{
    public class HttpRequestSender
    {
        private string mstrRequestUri = null;
        private string mstrContentType = "application/x-www-form-urlencoded";
        private bool mblnUseUTF8Encoding = false;

        private int _mintTimeoutInMillisconds = 0; // Will have no impacy if < 1

        public string ContentType
        {
            get
            {
                return mstrContentType;
            }
            set
            {
                mstrContentType = value;
            }
        }
        
        public bool UseUTF8Encoding
        {
            get
            {
                return mblnUseUTF8Encoding;
            }
            set
            {
                mblnUseUTF8Encoding = value;
            }
        }

        public int TimeoutInMillisconds
        {
            get
            {
                return _mintTimeoutInMillisconds;
            }
            set
            {
                _mintTimeoutInMillisconds = value;
            }
        }

        public HttpRequestSender(string requestUri)
        {
            mstrRequestUri = requestUri;
        }

        public HttpRequestSender()
        {
            SetRequestUri();
        }

        public HttpRequestSender(int timeoutInMilliseconds)
        {
            _mintTimeoutInMillisconds = timeoutInMilliseconds;
            SetRequestUri();
        }

        private void SetRequestUri()
        {
            mstrRequestUri = "http://localhost/ClickASPTestSite/HttpRequestProcessor.aspx";
            if (ConfigurationManager.AppSettings["HttpSenderRequestUri"] != null)
            {
                mstrRequestUri = ConfigurationManager.AppSettings["HttpSenderRequestUri"];
            }
        }

        public System.Xml.XmlDocument SubmitHttpRequest(string xmlContent, bool expectResponse)
        {
            return SubmitHttpRequest(xmlContent, expectResponse, null, "POST");
        }

        public System.Xml.XmlDocument SubmitHttpRequest(string xmlContent, bool expectResponse, System.Net.NetworkCredential credentials, string method)
        {
            System.Xml.XmlDocument objXMLResponse = null;
            System.Net.HttpWebResponse objResponse = null;
            System.Net.HttpWebRequest objRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(mstrRequestUri);
            byte[] arrContent = null;
            System.IO.Stream objRequestStream = null;
            System.IO.Stream objResponseStream = null;

            // SOAP message
            // mblnUseUTF8Encoding = true
            // mstrContentType = "text/xml; charset=utf-8";  
            // OR
            // mstrContextType = "application/soap+xml; charset=utf-8"

            try
            {
                // Request
                if (!mblnUseUTF8Encoding)
                {
                    arrContent = new ASCIIEncoding().GetBytes(xmlContent);
                }
                else
                {
                    arrContent = System.Text.Encoding.UTF8.GetBytes(xmlContent);
                }
                objRequest.ContentType = mstrContentType; // "application/x-www-form-urlencoded";
                if (method == null || method == string.Empty)
                {
                    method = "POST";
                }
                objRequest.Method = method;
                objRequest.ContentLength = arrContent.Length;
                if (credentials != null)
                {
                    objRequest.UseDefaultCredentials = true;
                }
                else
                {
                    objRequest.UseDefaultCredentials = false;
                    objRequest.Credentials = credentials;
                }

                if (_mintTimeoutInMillisconds > 0)
                {
                    // Ability to control Timeout allowed for the Http request on a call to call basis.
                    objRequest.Timeout = _mintTimeoutInMillisconds;
                }

                objRequestStream = objRequest.GetRequestStream();
                objRequestStream.Write(arrContent, 0, arrContent.Length);
                objRequestStream.Close();

                // Response
                if (expectResponse)
                {
                    objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse();
                    objResponseStream = objResponse.GetResponseStream();
                    System.IO.StreamReader readStream = new System.IO.StreamReader(objResponseStream, Encoding.UTF8);
                    string strResponse = readStream.ReadToEnd();

                    objResponse.Close();
                    readStream.Close();

                    if (strResponse != string.Empty)
                    {
                        try
                        {
                            objXMLResponse = new System.Xml.XmlDocument();
                            objXMLResponse.LoadXml(strResponse);
                        }
                        catch
                        {
                            // May not be Xml e.g. WebDAV
                        }
                    }
                }
            }
            catch (Exception excE)
            {
                throw excE;
            }

            return objXMLResponse;
        }
    }
}
