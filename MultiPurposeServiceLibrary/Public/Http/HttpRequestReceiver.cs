using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Configuration;

namespace MultiPurposeService.Public.Http
{
    public partial class HttpRequestReceiver
    {
        public object ProcessRequest(HttpListenerContext httpContext, Interfaces.IHttpRequestProcessor requestProcessor, bool sendResponse)
        {
            System.IO.BinaryReader objReader = null;
            System.Xml.XmlDocument objDOM = null;
            byte[] arrInputBuffer = null;
            byte[] arrOutputBuffer = null;
            string strInputXML = string.Empty;
            object objResponse = null;

            if (httpContext.Request.ContentLength64 > 0 && httpContext.Request.InputStream.CanRead)
            {
                objReader = new System.IO.BinaryReader(httpContext.Request.InputStream);
                arrInputBuffer = new byte[httpContext.Request.ContentLength64];
                arrInputBuffer = objReader.ReadBytes(arrInputBuffer.Length);
                strInputXML = System.Text.ASCIIEncoding.UTF8.GetString(arrInputBuffer);
                objDOM = new System.Xml.XmlDocument();
                objDOM.LoadXml(strInputXML);

                // Process the Xml message
                objResponse = requestProcessor.Process(objDOM);
                if (sendResponse && objResponse != null)
                {
                    if (objResponse is string)
                    {
                        arrOutputBuffer = System.Text.Encoding.ASCII.GetBytes((string)objResponse);

                        // Send Response
                        httpContext.Response.ContentType = "text/xml";
                        httpContext.Response.OutputStream.Write(arrOutputBuffer, 0, arrOutputBuffer.Length);
                        httpContext.Response.OutputStream.Close();

                        // TODO: (Possibly) Audit trail for the Output message sent to client
                    }
                }
            }
            else
            {
                objResponse = httpContext.Request.Url.OriginalString;
            }

            return objResponse;
        }
    }
}