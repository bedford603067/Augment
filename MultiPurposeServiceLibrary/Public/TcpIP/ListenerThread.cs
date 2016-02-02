using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

using MultiPurposeService.Public.MultiThreading;
using MultiPurposeService.Public.Abstract;
using MultiPurposeService.Interfaces;
using MultiPurposeService.Internal;
using MultiPurposeService.Internal.Abstract;
using MultiPurposeService.Internal.TcpIP;

namespace MultiPurposeService.Public.TcpIP
{
	/// <summary>
	/// Summary description for Server.
	/// </summary>
	public class ListenerThread:Base,IServiceContainer
	{
		#region Private Fields

		private TcpListener mobjServer=null;
		private Thread mobjServerThread = null;
		private PollingThread mobjMonitor = null;
		private ArrayList marrClients = null;
		private int mintPortNo;

		private const int POLLING_INTERVAL_SECS = 30; // Interval in ms to Poll for Redundant Clients

		#endregion

		#region Public Properties and Fields

		// None at present

		#endregion

		#region Inter-Thread Communication

		public delegate object ParentCallback(object sender,object Data);
		public delegate void HandleException(object sender,Exception childException);
		public delegate void ParentFeedback(object sender,string Data);

		public event ParentCallback OnCallback;
		public event HandleException OnException;

		void InitialiseDelegates()
		{
			OnCallback += new ParentCallback(InvokeParentCallback);
			OnException += new HandleException(InvokeHandleException); 
		}

		public object InvokeParentCallback(object sender,object Data)
		{
			try
			{
				if (sender is IProcessor)
				{
					// Processor call (occurs from within Client)
				}
				else if(sender is Client)
				{
					// Client call
				}
				
				else if(sender is Housekeeper)
				{
					// Pass back updated list of Connected Clients
					return marrClients;
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

			return this;
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

		public ListenerThread(int portNo)
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
			
			// Cannot use 'foreach' as member var Readonly, cannot set = null
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

			if (mobjMonitor != null)
			{
				mobjMonitor.Shutdown();
				mobjMonitor = null;
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

		public void Start()
		{
			Start(true);
		}

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

			marrClients=new System.Collections.ArrayList();

			while (true)
			{
				// Accept Client Connection and start Communicator instance
				objConnection = mobjServer.AcceptTcpClient();
				// RaiseLocalEvent(Me, "New Client Connected")
				
				// Create Processor instance 
				// TODO - Would need dynamic assembly load HERE if going about this way
				// objProcessor = new IProcessor();

				/*
				// Wire Processor instance to call Server copy of method for OnCallback
				objProcessor.OnCallback -= new Processor.ParentCallback(objProcessor.InvokeParentCallback);
				objProcessor.OnCallback += new Processor.ParentCallback(this.InvokeParentCallback);

				*/

				// Create Child Client instance 
				objClient = new Client(objConnection, objProcessor);

				// Wire child Client class to call Server copy of method for OnCallback
				objClient.OnCallback -= new Client.ParentCallback(objClient.InvokeParentCallback);
				objClient.OnCallback += new Client.ParentCallback(this.InvokeParentCallback);
				
				// Wire child Client class to Pass Exceptions up to Server copy 
				objClient.OnException -= new Client.HandleException(objClient.InvokeHandleException);
				objClient.OnException += new Client.HandleException(this.InvokeHandleException);

				// Add child Client instance to Array and start work on Client connection
				intArrIndex = marrClients.Add(objClient);
				objClient.Start();

				// Start background Thread that polls for Redundant Clients
				MonitorClients();

			}
		}

		private void MonitorClients()
		{
			Housekeeper objHousekeeper = null;

			// Note. Dedicated to removing disconnected and otherwise dormant Tcp Clients

			objHousekeeper = new Housekeeper();
			objHousekeeper.OnCallback-= new Housekeeper.ParentCallback(objHousekeeper.InvokeParentCallback);
			objHousekeeper.OnCallback+= new Housekeeper.ParentCallback(this.InvokeParentCallback);

			mobjMonitor=new PollingThread(objHousekeeper,POLLING_INTERVAL_SECS,"Thread_Housekeeper");
			mobjMonitor.Start();

		}

		#endregion

		#region Private Classes

		public class Housekeeper:Base,IProcessor
		{
			
			public delegate object ParentCallback(object sender,object Data);
			public delegate void HandleException(object sender,Exception childException);

			public event ParentCallback OnCallback;
			public event HandleException OnException;

			public object InvokeParentCallback(object sender,object Data)
			{
				try
				{
					OnCallback(sender,Data);
				}
				catch (Exception e)
				{
					OnException(this,e);
				}

				return this;
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

			#region Private Fields

			ArrayList marrConnectedClients = null;

			#endregion

			#region Constructors

			public Housekeeper()
			{
				// No work to do in Constructor
			}

			#endregion

			#region Public Properties

			public ArrayList ConnectedClients
			{
				get
				{
					return marrConnectedClients;
				}
				set
				{
					marrConnectedClients = value;
				}
			}

			#endregion

			#region Public Methods

			public void Process(object inputData)
			{
				int intArrIndex;
			
				// Note. Removes disconnected and dormant Tcp Clients

				// Cannot use 'foreach' as member var Readonly, cannot set = null
				if (marrConnectedClients != null)
				{
					for (intArrIndex=0;intArrIndex < marrConnectedClients.Count;intArrIndex++)
					{
						if (((Client) marrConnectedClients[intArrIndex]).Status == Client.eClientStatus.Disconnected || 
							((Client)marrConnectedClients[intArrIndex]).Status == Client.eClientStatus.Dormant)
						{
							((Client) marrConnectedClients[intArrIndex]).Shutdown();
							marrConnectedClients[intArrIndex]=null;
							marrConnectedClients.RemoveAt(intArrIndex);
						}
					}
				}

				// Passs updated Client set back to Caller thread
				marrConnectedClients=((ArrayList)this.OnCallback(this,((object)marrConnectedClients)));

			}

			public void Shutdown()
			{
				Console.WriteLine("No Problem");
			}

			#endregion

		}
		

		#endregion

	}
}
