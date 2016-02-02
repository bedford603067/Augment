using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Configuration;

using MultiPurposeService.Internal;

namespace MultiPurposeService.Public.Http
{
    public class HttpListenerService
    {
        #region Private Fields

        private HttpListener mobjListener;
        private Interfaces.IHttpRequestProcessor mobjMessageProcessor;
        private System.Timers.Timer mobjTimer = null;
        private int mintTimerIntervalInMs = 60000;
        private bool mblnForceCloseOfResponseStream = true; //
        private bool mblnSendXmlReceiptToResponseStream = false;

        #endregion

        #region Public Properties

        public Interfaces.IHttpRequestProcessor MessageProcessor
        {
            get
            {
                return mobjMessageProcessor;
            }
            set
            {
                mobjMessageProcessor = value;
            }
        }

        public bool ForceCloseOfResponseStream
        {
            get
            {
                return mblnForceCloseOfResponseStream;
            }
            set
            {
                mblnForceCloseOfResponseStream = value;
            }
        }

        public bool SendXmlReceiptToResponseStream
        {
            get
            {
                return mblnSendXmlReceiptToResponseStream;
            }
            set
            {
                mblnSendXmlReceiptToResponseStream = value;
            }
        }

        #endregion

        #region Inter-Thread Communication

        public delegate object ParentCallback(object sender, object state);
        public delegate void HandleException(object sender, Exception childException);

        public event ParentCallback OnCallback;
        public event HandleException OnException;

        void InitialiseDelegates()
        {
            OnCallback += new ParentCallback(InvokeParentCallback);
            OnException += new HandleException(InvokeHandleException);
        }

        public object InvokeParentCallback(object sender, object Data)
        {
            //SendToWorkManager(sender, Data);
            return (object)true;
        }

        public void InvokeHandleException(object sender, Exception childException)
        {
            // Event Log
            Logging.WriteToLog(this,childException);

            // Expose error to any callers who may be wired in to this event as well
            OnException(sender, childException);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// This is simply used to as a Keep Alive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mobjTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            mobjTimer.Enabled = false;
            mobjTimer.Enabled = true;
        }

        #endregion

        #region Public Methods

        public HttpListenerService(Interfaces.IHttpRequestProcessor messageProcessor)
        {
            mobjMessageProcessor = messageProcessor;
        }

        public void Start()
        {
            string strHttpListenerAddress = "http://localhost:8080/ClickListener/";
            int intMaxNoOfThreads = 20;

            try
            {
                if (ConfigurationManager.AppSettings["HttpListenerAddress"] != null)
                {
                    strHttpListenerAddress = ConfigurationManager.AppSettings["HttpListenerAddress"];
                }
                if (ConfigurationManager.AppSettings["HttpListenerMaxNoOfThreads"] != null)
                {
                    intMaxNoOfThreads = int.Parse(ConfigurationManager.AppSettings["HttpListenerMaxNoOfThreads"]);
                }
                Start(strHttpListenerAddress, intMaxNoOfThreads);
            }
            catch (Exception excE)
            {
                if (OnException != null)
                {
                    // Inform event subscribers of the Exception encountered
                    OnException("HttpListenerService_Start", excE);
                }
                else
                {
                    throw excE;
                }
            }
        }

        public void Start(string httpListenerAddress, int maxNoOfThreads)
        {
            try
            {
                mobjListener = new HttpListener();
                mobjListener.Prefixes.Add(httpListenerAddress);
                mobjListener.Start();

                // Set off intMaxNoOfThreads to listen for messages at httpListenerAddress
                for (int intIndex = 0; intIndex < maxNoOfThreads; intIndex++)
                {
                    mobjListener.BeginGetContext(new AsyncCallback(ReceiveRequest), mobjListener);
                }
                Console.WriteLine("HTTP Server at " + httpListenerAddress + " is accepting incoming messages");

                // Keep alive
                mobjTimer = new System.Timers.Timer(mintTimerIntervalInMs);
                mobjTimer.Elapsed += new System.Timers.ElapsedEventHandler(mobjTimer_Elapsed);
                mobjTimer.Enabled = true;
            }
            catch (Exception excE)
            {
                if (OnException != null)
                {
                    // Inform event subscribers of the Exception encountered
                    OnException("HttpListenerService_Start", excE);
                }
                else
                {
                    throw excE;
                }
            }
        }

        private void ReceiveRequest(IAsyncResult result)
        {
            HttpListenerContext objContext;
            HttpRequestReceiver objProcessor = new HttpRequestReceiver();
            object objRequest = null;

            if (mobjListener == null)
            {
                return;
            }

            try
            {
                // Get the input stream (within HttpContext)
                objContext = mobjListener.EndGetContext(result);
                // Spin up a replacement thread for the one Request has just been received on
                mobjListener.BeginGetContext(new AsyncCallback(ReceiveRequest), mobjListener);
                // Process received Message
                objRequest = objProcessor.ProcessRequest(objContext, mobjMessageProcessor, false);
                // Close the thread if appropriate
                if (mblnForceCloseOfResponseStream)
                {
                    if (mblnSendXmlReceiptToResponseStream)
                    {
                        string xmlReceipt = "<HttpListenerResponse><Outcome>1</Outcome></HttpListenerResponse>";
                        byte[] xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlReceipt);
                        objContext.Response.Close(xmlBytes, false);
                    }
                    else
                    {
                        objContext.Response.Close();
                    }
                }
                // Inform event subscribers of the Message that has been received
                if (OnCallback != null && objRequest!=null)
                {
                    OnCallback("HttpListenerService", objRequest);
                }
            }
            catch (Exception excE)
            {
                if (OnException != null)
                {
                    // Inform event subscribers of the Exception encountered
                    OnException("HttpListenerService_ReceiveRequest", excE);
                }
                else
                {
                    throw excE;
                }
            }
        }

        public void Stop()
        {
            if (mobjListener != null)
            {
                mobjListener.Stop();
                mobjListener = null;
                GC.Collect();
            }
        }

        //private void ProcessRequest(HttpListenerContext httpContext)
        //{
        //    byte[] arrOutputBuffer;
        //    string strResponse;
        //    string strSender;
        //    string strInputMessage;

        //    strSender = httpContext.Request.QueryString["sender"];
        //    strInputMessage = httpContext.Request.QueryString["msg"];
        //    Console.WriteLine(strSender + " has sent the following message: " + strInputMessage);

        //    // Send Response back to client
        //    strResponse = "{" + strSender + "}{Thank you for your message}";
        //    arrOutputBuffer = System.Text.Encoding.UTF8.GetBytes(strResponse);
        //    httpContext.Response.ContentLength64 = arrOutputBuffer.Length;
        //    httpContext.Response.ContentType = "text/html; charset=utf-8";
        //    httpContext.Response.OutputStream.Write(arrOutputBuffer, 0, arrOutputBuffer.Length);
        //    httpContext.Response.Close();
        //}

        #endregion
    }
}
