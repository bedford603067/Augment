using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

using MultiPurposeService.Public.Abstract;

namespace MultiPurposeService.Internal.TcpIP
{
	/// <summary>
	/// Summary description for SocketClient.
	/// </summary>
	public abstract class SocketClient:Base
	{

		#region Private Fields

		private IPHostEntry mobjIPHostInfo;
		private IPAddress mobjIPAddress; 
		private Socket mobjClient;

		#endregion

		#region Public Properties and Fields

		public bool IsConnected
		{
			get
			{
				if (mobjClient != null)
				{
					/*
					System.Net.Sockets.SelectMode objMode = System.Net.Sockets.SelectMode.SelectError;
					return mobjClient.Poll(0,objMode);
					*/
					return mobjClient.Connected;
				}
				else
				{
					return false;
				}
			}						  
		}

		public bool StayAlive = false;
		public System.TimeSpan Timeout = new TimeSpan(0,0,0,45,0); // Default 45 seconds

		#endregion

		#region Public Events

		public delegate void ReceiveData(string responseData);
		public event ReceiveData DataReceived;

		public delegate void RaiseException(Exception remoteException);
		public event RaiseException ExceptionEncountered;

		#endregion

		#region Construct\Finalise
		
		public SocketClient(SocketConnection socketConnection) 
		{
			Initialise(socketConnection.IPAddress,socketConnection.PortNo);
		}

		public SocketClient(string hostAddress, int portNumber) 
		{
			Initialise(hostAddress,portNumber);
		}

		public SocketClient(int portNumber) 
		{
			Initialise(IPAddress.Loopback.ToString(),portNumber);
		}

		public SocketClient() 
		{
			//
		}

		protected override void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Release managed resources.
			}
			// Release unmanaged resources.
			if (mobjClient != null)
			{
				if (mobjClient.Connected == true)
				{
					mobjClient.Shutdown(SocketShutdown.Both);
					mobjClient.Close();
				}
				mobjClient = null;
			}

			// Call Dispose on Base
			base.Dispose(disposing);
		}

		#endregion

		#region Public Methods

		#region Asynchronous

		protected void Connect(EndPoint remoteEP)
		{
			System.IAsyncResult result;
			Exception objException;

			try
			{
				result = mobjClient.BeginConnect(remoteEP,null,null);
				if (result.AsyncWaitHandle.WaitOne(Timeout,false)==true)
				{
					// OK. Alternatively the call attempt Timed out
					mobjClient.EndConnect(result);
				}
			}
			catch (Exception e)
			{
				objException=new Exception("Unable to Connect to target Socket",e);
				objException.Source="SocketClient";
				throw objException;
				// ExceptionEncountered(objException);
			}
		}

		
		/*

		protected delegate void ConnectDelegate(EndPoint remoteEP);

		protected void Connect3(EndPoint remoteEP)
		{
			ConnectDelegate ptrConnect;
			System.IAsyncResult result;

			try
			{
				ptrConnect = new ConnectDelegate(Connect);
				result = ptrConnect.BeginInvoke(remoteEP,null,null);
				ptrConnect.EndInvoke(result);
			}
			catch (Exception e)
			{
				ExceptionEncountered(e);
			}
		}

		*/

		

		protected void Send(string data, bool autoReceive)
		{
			System.IAsyncResult result;
			byte[] byteData;
			
			try
			{
				byteData = Encoding.ASCII.GetBytes(data);
				result = mobjClient.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,null,null);
				if (result.AsyncWaitHandle.WaitOne(Timeout,false))
				{
					// OK. Alternatively the call Timed out
					mobjClient.EndSend(result);

					if (autoReceive == true)
					{
						// Prepare to receive further data on this connection
						Receive(true);
					}
				}
			}
			catch (Exception e)
			{
				// mobjClient.Shutdown(SocketShutdown.Send)
				ExceptionEncountered(e);
			}

		}

		
		/*
		protected string Receive(bool respondViaEvent)
		{
			System.IAsyncResult result;
			byte[] bytes = new byte[1024];
			string strResponse = null;

			try
			{
				result = mobjClient.BeginReceive(bytes,0,bytes.Length,System.Net.Sockets.SocketFlags.None,null,null);
				if (result.AsyncWaitHandle.WaitOne(Timeout,false))
				{
					// OK. Alternatively the call Timed out
					mobjClient.EndReceive(result);

					strResponse = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
					if (respondViaEvent == true)
					{
						DataReceived(strResponse);
					}
				}
			}
			catch (Exception e)
			{
				//mobjClient.Shutdown(SocketShutdown.Receive)
				ExceptionEncountered(e);
			}

			return strResponse;

		}

		*/
		
		#endregion

		#region Synchronous
		
		/*
		protected void Connect(EndPoint remoteEP)
		{
			try
			{
				mobjClient.Connect(remoteEP);
			}
			catch (Exception e)
			{
				ExceptionEncountered(e);
			}
		}
		
		protected void Send(string data, bool autoReceive)
		{
			byte[] byteData;
			int bytesSent;

			try
			{
				byteData = Encoding.ASCII.GetBytes(data);
				bytesSent = mobjClient.Send(byteData, 0, byteData.Length, SocketFlags.None);
				if (autoReceive == true)
				{
					Receive(true);
				}
			}
			catch (Exception e)
			{
				// mobjClient.Shutdown(SocketShutdown.Send)
				ExceptionEncountered(e);
			}

		}
		
		*/

		protected string Receive(bool respondViaEvent)
		{
			byte[] bytes = new byte[1024];
			string strResponse = null;
			int bytesRec;

			try
			{
				bytesRec = mobjClient.Receive(bytes);
				strResponse = Encoding.ASCII.GetString(bytes, 0, bytesRec);
				if (respondViaEvent == true)
				{
					DataReceived(strResponse);
				}
			}
			catch (Exception e)
			{
				//mobjClient.Shutdown(SocketShutdown.Receive)
				ExceptionEncountered(e);
			}

			return strResponse;

		}

		#endregion

		#endregion

		#region Private methods

		public void Initialise(string hostAddress, int portNumber) 
		{
			try
			{
				mobjIPHostInfo = Dns.Resolve(hostAddress);
				mobjIPAddress = mobjIPHostInfo.AddressList[0];
				IPEndPoint ipe = new IPEndPoint(mobjIPAddress, portNumber);
				mobjClient = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
				Connect(ipe);
			}
			catch (Exception excE)
			{
				throw excE;
			}
		}

		#endregion

		#region Public Classes

		public class SocketConnection
		{
			public string IPAddress;
			public int PortNo;
		}

		#endregion

	}
}







