using System;
using System.Collections;
using System.ComponentModel;

using System.Configuration;
using System.Diagnostics;
using System.Threading;

using MultiPurposeService.Public.Abstract;
using MultiPurposeService.Internal;
using MultiPurposeService.Internal.Service;
using MultiPurposeService.Internal.TcpIP;

namespace MultiPurposeService.Public.Service
{
	/// <summary>
	/// MAIN THREAD:
	/// Server instances(Tcp Listeners maintained in an ArrayList)
	/// ASYNCHRONOUS THREADS
	/// Server child classes (Client and Processor:SocketClient)
	/// BACKGROUND THREADS :
	/// Processor		(processes received work requests, holds state)
	/// BatchAgent		(queues Transactions, processes queued Transactions, holds state)
	/// BatchMonitor	(post processing of Transactions, updates secondary Queue, send Alerts)
	/// OWN PROCESS:
	/// External Executables		
	/// </summary>

	public class ServiceManager:Base,MultiPurposeService.Interfaces.IServiceContainer
	{

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

		public object InvokeParentCallback(object sender,object Data)
		{
			SendToWorkManager(sender, Data);
			return (object)true;
		}

		public void InvokeHandleException(object sender, Exception childException)
		{
			// Event Log
			Logging.WriteToLog(this, childException);

			// Expose error to any callers who may be wired in to this event as well
			OnException(sender,childException);
		}

		#endregion

		#region Private Fields

		// Main Threads
		private Thread mobjWorkManagerThread = null;
		private Processor mobjWorkManager = null;
		private Internal.TcpIP.Server[] marrServers = null;

		// Batch Processing
		private Thread mobjBatchThread = null;
		private BatchAgent mobjBatchAgent = null;
		//private SqlBatchAgent mobjSqlBatchAgent = null;

		// Post Batch Processing
		private Thread mobjMonitorThread = null;
		//private BatchMonitor mobjBatchMonitor = null;

		#endregion

		#region Construct\Finalise

		public ServiceManager()
		{
			// Log Exceptions and User Messages
            Logging.WriteToLog(this, "Service started successfully");
		}

		protected override void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Release managed resources.
			}
			// Release local resources.

			/*
			// Kill external processes
			if (mobjExternalProcess != null)
			{
				mobjExternalProcess.Kill();
				mobjExternalProcess= null;
			}
			*/

			// Work Manager disposal
			if (mobjWorkManager != null)
			{
				mobjWorkManager.Shutdown();
				mobjWorkManager = null;
			}

			// Batch Agent disposal
			if (mobjBatchThread != null)
			{
				mobjBatchThread.Abort();
				mobjBatchThread=null;
			}
			if (mobjBatchAgent != null)
			{
				mobjBatchAgent.Shutdown();
				mobjBatchAgent=null;
			}

			// Batch Monitor disposal
			if (mobjMonitorThread != null)
			{
				mobjMonitorThread.Abort();
				mobjMonitorThread=null;
			}
			//if (mobjBatchMonitor != null)
			//{
			//    mobjBatchMonitor.Shutdown();
			//    mobjBatchMonitor=null;
			//}

			// Destroy Tcp Servers, including all their Tcp Clients and asynch threads
			if (marrServers != null)
			{
				foreach (Server objServer in marrServers)
				{
					objServer.Shutdown();
				}
				marrServers=null;
			}

			//// Call Dispose on Base
			//Base.Dispose(disposing);
		}

		public void Shutdown()
		{
			Dispose();
		}


		#endregion

		#region Public Methods

		public void Start()
		{
			/* Work Manager */
			// Create a Processor instance that will physically process all incoming work
			mobjWorkManagerThread = ConfigureWorkManager(true); 
			/* Tcp Listeners */
			// Listens for incoming connections and passes received data to Work Manager for processing
			marrServers = ConfigureTcpServers();
			foreach (Server objServer in marrServers)
			{
				objServer.Start(true);
			}
		}

		#endregion

		#region Private Methods

		#region Batch Agent

		private Thread ConfigureWorkManager(bool startAtOnce)
		{
			// Create a Processor instance that will physically process all incoming work
			mobjWorkManager = new Processor(); 

			// Wire child Work Manager class to Pass Exceptions up to Service instance 
			mobjWorkManager.OnException -= new Processor.HandleException(mobjWorkManager.InvokeHandleException);
			mobjWorkManager.OnException += new Processor.HandleException(this.InvokeHandleException);

			// Wire Service instance to call Work Manager copy of method for OnCallback
			// The main Service thread passes thru requests from Servers to the Work Manager
			this.OnCallback -= new ParentCallback(this.InvokeParentCallback);
			this.OnCallback += new ParentCallback(mobjWorkManager.InvokeParentCallback);

			// Allocate Agent to thread
			mobjWorkManagerThread = new Thread(new ThreadStart(mobjWorkManager.Process));

			// Run on a background thread
			mobjWorkManagerThread.Name = "Thread_WorkManager";
			mobjWorkManagerThread.IsBackground = true;
			if (startAtOnce == true)
			{
				mobjWorkManagerThread.Start();
			}

			return mobjWorkManagerThread;
		}

		private void SendToWorkManager(object sender, object inputData)
		{
			mobjWorkManager.Process(inputData);
		}

		#endregion

		#region TcpIP Communication 

		private Internal.TcpIP.Server[] ConfigureTcpServers()
		{
			/* Servers (TcpListeners)  */
			// Create Server instance(s) on background threads

			Server objServer;
			ArrayList colServers = new ArrayList();
			string strPortList;
			string[] arrPorts;
			
			strPortList = ConfigurationManager.AppSettings.Get("TcpListenerPorts");
			arrPorts = strPortList.Split(",".ToCharArray(), 20);
			foreach (string strPortNo in arrPorts)
			{
				objServer = new Server(int.Parse(strPortNo));
				//objServer.Connection = mobjSocketConnection;

				// Wire Server instance to call Service copy of method for OnCallback
				objServer.OnCallback -= new Server.ParentCallback(objServer.InvokeParentCallback);
				objServer.OnCallback += new Server.ParentCallback(this.InvokeParentCallback);

				// Wire child Server class to Pass Exceptions up to Service instance 
				objServer.OnException -= new Server.HandleException(objServer.InvokeHandleException);
				objServer.OnException += new Server.HandleException(this.InvokeHandleException);

				// Keep Server instances in local Array
				colServers.Add(objServer);

				// Begin Work
				// objServer.Start(true);

			}

			if (colServers.Count > 0)
			{
				marrServers = (Server[])colServers.ToArray(typeof(Server));
			}

			return marrServers;
		}

		#endregion

		#region Batch Agent

		private Thread ConfigureBatchAgent(Processor processorInstance, bool startAtOnce)
		{
			string strInboundQ, strOutboundQ;
			string strQueueReceiveSP;
			string strQueueSendSP;
			string strQueueRemoveSP;
			bool blnUseSqlAgent = false;

			// Work out which Batch Agent type to use, pass the Processor to it, and configure inter-class calls
			blnUseSqlAgent = bool.Parse(ConfigurationManager.AppSettings.Get("UseSqlBatchAgent"));
			if (blnUseSqlAgent == false)
			{
				// Create instance of the Message Queue Based Agent that will perform Batch Mode processing
                strInboundQ = ConfigurationManager.AppSettings.Get("InboundQueuePath");
                strOutboundQ = ConfigurationManager.AppSettings.Get("OutboundQueuePath");
				mobjBatchAgent = new BatchAgent(strInboundQ, strOutboundQ, processorInstance);

				// Configure Polling Interval at which BatchAgent queries Inbound Queue
                if (ConfigurationManager.AppSettings.Get("InboundPollingInterval") != null)
				{
					int intPollingMinutes;
                    intPollingMinutes = int.Parse(ConfigurationManager.AppSettings.Get("InboundPollingInterval"));
					mobjBatchAgent.PollingInterval = new TimeSpan(0, 0, intPollingMinutes, 0, 0);
				}

				// Wire child BatchAgent class to Pass Exceptions up to Service instance 
				mobjBatchAgent.OnException -= new BatchAgent.HandleException(mobjBatchAgent.InvokeHandleException);
				mobjBatchAgent.OnException += new BatchAgent.HandleException(this.InvokeHandleException);

				// Wire Service instance to call BatchAgent copy of method for OnCallback
				// The main Service thread passes thru requests from Servers to the Batch Agent
				this.OnCallback -= new ParentCallback(this.InvokeParentCallback);
				this.OnCallback += new ParentCallback(mobjBatchAgent.InvokeParentCallback);

				// Allocate Agent to thread
				mobjBatchThread = new Thread(new ThreadStart(mobjBatchAgent.Poll));
			}
			else
			{
				//// Create instance of the Sql Server Based Agent that will perform Batch Mode processing
				//strQueueReceiveSP = ConfigurationSettings.AppSettings.Get("QueueReceiveSP");
				//strQueueSendSP = ConfigurationSettings.AppSettings.Get("QueueSendSP");
				//strQueueRemoveSP = ConfigurationSettings.AppSettings.Get("QueueRemoveSP");
				//mobjSqlBatchAgent = new SqlBatchAgent(strQueueReceiveSP,strQueueSendSP,strQueueRemoveSP,objProcessor);

				//// Configure Polling Interval at which SqlBatchAgent queries SQL Server Queue table
				//if (ConfigurationSettings.AppSettings.Get("SqlQueuePollingInterval") != null)
				//{
				//    int intPollingMinutes;
				//    intPollingMinutes=int.Parse(ConfigurationSettings.AppSettings.Get("SqlQueuePollingInterval"));
				//    mobjSqlBatchAgent.PollingInterval=new TimeSpan(0,0,intPollingMinutes,0,0);
				//}

				//// Wire Service instance to call BatchAgent copy of method for OnCallback
				//// The main Service thread passes thru requests from Servers to the Batch Agent
				//this.OnCallback-= new ParentCallback(this.InvokeParentCallback);
				//this.OnCallback+= new ParentCallback(mobjSqlBatchAgent.InvokeParentCallback);

				//// Wire child SqlBatchAgent class to Pass Exceptions up to Service instance 
				//mobjSqlBatchAgent.OnException -= new SqlBatchAgent.HandleException(mobjSqlBatchAgent.InvokeHandleException);
				//mobjSqlBatchAgent.OnException += new SqlBatchAgent.HandleException(this.InvokeHandleException);

				//// Allocate Agent to thread
				//mobjBatchThread = new Thread(new ThreadStart(mobjSqlBatchAgent.Poll));
			}

			// Run on a background thread
			mobjBatchThread.Name = "Thread_BatchAgent";
			mobjBatchThread.IsBackground = true;
			if (startAtOnce == true)
			{
				mobjBatchThread.Start();
			}

			return mobjBatchThread;
		}

		private object SendToBatchAgent(object sender,object Data)
		{
			object objStatus = null;
			BatchAgent.eCommideaStatus eStatus = BatchAgent.eCommideaStatus.Offline;

			objStatus= OnCallback(sender,Data);
			eStatus = (BatchAgent.eCommideaStatus) objStatus;

			return objStatus;
		}

		#endregion

		#region Other Services


		/* Post-Batch Processing */

		//// Create instance of the Monitor that will perform Post-Batch processing
		//strOutboundQ = ConfigurationSettings.AppSettings.Get("OutboundQueuePath");
		//mobjBatchMonitor = new BatchMonitor(strOutboundQ);

		//// Configure Polling Interval at which BatchAgent queries Inbound Queue
		//if (ConfigurationSettings.AppSettings.Get("OutboundPollingInterval") != null)
		//{
		//    int intPollingMinutes;
		//    intPollingMinutes=int.Parse(ConfigurationSettings.AppSettings.Get("OutboundPollingInterval"));
		//    mobjBatchMonitor.PollingInterval=new TimeSpan(0,0,intPollingMinutes,0,0);
		//}

		/*
		// Start external applications required if not currently running
		Process[] arrProcesses;
		string strExecutablePath = "";
		string strAppName = "ExternalApplication";

		arrProcesses = Process.GetProcessesByName(strAppName);
		if (arrProcesses.Length == 0)
		{
			arrProcesses = null;
			strWinTiPath += ConfigurationSettings.AppSettings.Get(strAppName);
			mobjApp = new AppAgent(strExecutablePath);
			mobjApp.Start();
		}
		//System.Threading.Thread.Sleep(5000);
		*/

		/*--------------------------------------------------------------------------------------*/
		
		/*
		// Configure socket connection required. May be used by Processor (inherits SocketClient)
		mobjSocketConnection = new Processor.SocketConnection();
		mobjSocketConnection.IPAddress=System.Net.IPAddress.Loopback.ToString();
		mobjSocketConnection.PortNo=int.Parse(ConfigurationSettings.AppSettings.Get("OutboundPort"));
		*/

		//// Wire child BatchMonitor class to Pass Exceptions up to Service instance 
		//mobjBatchMonitor.OnException -= new BatchMonitor.HandleException(mobjBatchMonitor.InvokeHandleException);
		//mobjBatchMonitor.OnException += new BatchMonitor.HandleException(this.InvokeHandleException);

		//// Run on a background thread
		//mobjMonitorThread = new Thread(new ThreadStart(mobjBatchMonitor.Poll));
		//mobjMonitorThread.Name = "Thread_BatchMonitor";
		//mobjMonitorThread.IsBackground = true;
		//mobjMonitorThread.Start();


		#endregion

		#endregion

	}
}