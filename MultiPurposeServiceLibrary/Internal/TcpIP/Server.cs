using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

using MultiPurposeService.Public.Abstract;

namespace MultiPurposeService.Internal.TcpIP
{
	/// <summary>
	/// Summary description for Server.
	/// </summary>
	internal class Server:Base
	{
		#region Private Fields

        private TcpListener mobjServer=null;
        private Thread mobjServerThread;
		private System.Collections.ArrayList marrClients = null;
		private Processor.SocketConnection mobjSocketConnection = null;
		private int mintPortNo;

		#endregion

		#region Public Properties and Fields

		public Processor.SocketConnection Connection
		{
			get
			{
				return mobjSocketConnection;
			}
			set
			{
				mobjSocketConnection = value ;
			}
		}

		#endregion

		#region Inter-Thread Communication

		public delegate object ParentCallback(object sender,object state);
		public delegate void HandleException(object sender,Exception childException);
		public delegate void ParentFeedback(object sender,string state);

		public event ParentCallback OnCallback;
		public event HandleException OnException;

		void InitialiseDelegates()
		{
			OnCallback += new ParentCallback(InvokeParentCallback);
			OnException += new HandleException(InvokeHandleException); 
		}

		public object InvokeParentCallback(object sender,object state)
		{
			try
			{
				if (sender is Processor)
				{
					// Processor call (occuring from within Client)

					/*
					// Action - Find out if Service is in Batch Mode
					if (state is KnownNamespace.KnownType)
					{
						 state = OnCallback(sender,state);
					}
					*/

				}
				else if(sender is Client)
				{
					// Client call - Forward to container instance (ServiceManager)
					state = OnCallback(sender, state);
				}
				else
				{
					// Local call
				}
			}
			catch (Exception e)
			{
				OnException(this,e);
			}

			return state;
		}

		public void InvokeHandleException(object sender,Exception childException)
		{
			if (OnException != null)
			{
				OnException(sender,childException);
			}
			else
			{
				throw childException;
			}
		}

		#endregion

		#region Construct\Finalise

		public Server(int portNo)
		{
			mintPortNo = portNo;
		}

		protected override void Dispose(bool disposing) 
		{
			int intArrIndex = 0 ;

			if (disposing) 
			{
				// Release managed resources.
			}
			// Release local resources.
			
			// Note. Cannot use 'foreach' as member var readonly and so could not set = null
			if (marrClients != null)
			{
				for (intArrIndex=0;intArrIndex < marrClients.Count;intArrIndex++)
				{
					if (marrClients[intArrIndex] != null)
					{
						((Client) marrClients[intArrIndex]).Shutdown();
						marrClients[intArrIndex]=null;
					}
				}
				marrClients= null;
			}

			if (mobjServerThread != null)
			{
				mobjServerThread.Abort();
				mobjServer.Stop();
				mobjServerThread = null;
			}

			// Call Dispose on Base
			base.Dispose(disposing);
		}

		public void Shutdown()
		{
			Dispose(true);

		}

		#endregion

		#region Public Methods

		public void Start(bool runOnBackgroundThread)
		{
			IPEndPoint objEndPoint;
			int intAvailableThreads;
			int intAvailableAsynchThreads;
		
			try
			{
				// Note. Use System.Net.IPAddress.Any - other approaches only work with client on same box
				objEndPoint=new IPEndPoint(System.Net.IPAddress.Any,mintPortNo);
				mobjServer = new TcpListener(objEndPoint);

				// Listener as Synchronous or Asynchronous Process
				ThreadPool.GetAvailableThreads(out intAvailableThreads, out intAvailableAsynchThreads);

				if (runOnBackgroundThread == false || intAvailableThreads == 0)
				{
					// Synchronous
					mobjServer.Start();
					AcceptConnections();
				}
				else
				{
					// Asynchronous 
					try
					{
						if (mobjServerThread != null)
						{
							// Shutdown thread, invalid attempt to call a second time
						}
						mobjServerThread = new Thread(new ThreadStart(Listen));
						mobjServerThread.Start();
					}
					catch (SocketException excS)
					{
						throw excS;
					}
				}
			}
			catch
			{
				throw;
			}
		}


		#endregion

		#region Private Methods

		private void Listen()
		{
			mobjServerThread.Name = "Listener_" + mintPortNo.ToString();
			mobjServer.Start();
			AcceptConnections();
		}

		private void AcceptConnections()
		{
			TcpClient objConnection;
			Client objClient;
			Processor objProcessor = null;
			int intArrIndex;
			bool blnForwardClientRequests = false;

			marrClients=new System.Collections.ArrayList();

			while (true) // Put this on a timer if more resources needed between times
			{
				// Accept Client Connection and start Communicator instance
				objConnection = mobjServer.AcceptTcpClient();
				if (objConnection != null)
				{
					Logging.WriteToLog(this, "New Client Connected");

					if (ConfigurationManager.AppSettings["ForwardClientRequests"] != null)
					{
						blnForwardClientRequests = bool.Parse(ConfigurationManager.AppSettings["ForwardClientRequests"]);
					}
					if (blnForwardClientRequests == false)
					{
						// Create Processor instance 
						objProcessor = new Processor(); // new Processor(mobjSocketConnection)

						// Wire Processor instance to call Server copy of method for OnCallback
						objProcessor.OnCallback -= new Processor.ParentCallback(objProcessor.InvokeParentCallback);
						objProcessor.OnCallback += new Processor.ParentCallback(this.InvokeParentCallback);
					}

					// Create Child Client instance 
					objClient = new Client(objConnection, objProcessor); // Pass non empty Processor instance for immediate work

					// Wire child Client class to call Server copy of method for OnCallback
					objClient.OnCallback -= new Client.ParentCallback(objClient.InvokeParentCallback);
					objClient.OnCallback += new Client.ParentCallback(this.InvokeParentCallback);

					// Wire child Client class to Pass Exceptions up to Server copy 
					objClient.OnException -= new Client.HandleException(objClient.InvokeHandleException);
					objClient.OnException += new Client.HandleException(this.InvokeHandleException);

					// Add child Client instance to Array and start work on Client connection
					intArrIndex = marrClients.Add(objClient);
					objClient.Start();

					//((Client) marrClients(intArrIndex)).Start();
				}
			}
		}

		#endregion

	}
}
