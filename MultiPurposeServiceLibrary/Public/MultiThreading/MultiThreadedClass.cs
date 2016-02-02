using System;
using System.Collections.Generic;
using System.Text;

using MultiPurposeService.Internal;

namespace MultiPurposeService.Public.MultiThreading
{
	public class MultiThreadedClass
	{
		#region Public Fields

		public volatile bool HasStopWorkRequestBeenReceived = false;
		public volatile bool IsBusy = false;

		#endregion

		#region Public Methods

		public void ReceiveStopRequest(MultiThreadedClass sender)
		{
			// Accept call from Parent instance post the hosting Thread having started
			HasStopWorkRequestBeenReceived = true;
		}

		public object TransferState(MultiThreadedClass sender, object state)
		{
			// Accept call from Parent instance post the hosting Thread having started
			// Can respond to incoming state including return data to hosting thread
			return null;
		}

		#endregion 

		#region Logging

		internal System.Diagnostics.EventLog ServiceEventLog = new System.Diagnostics.EventLog();
		internal string ServiceName = "TaskManager";
		internal string EVENT_LOG_NAME = "Services";

		internal void ConfigureLog()
		{
			try
			{
				if (System.Diagnostics.EventLog.SourceExists(ServiceName) == true)
				{
					System.Diagnostics.EventLog.DeleteEventSource(ServiceName);
				}
				System.Diagnostics.EventLog.CreateEventSource(ServiceName, EVENT_LOG_NAME);

				ServiceEventLog.Source = ServiceName;
				ServiceEventLog.Log = EVENT_LOG_NAME;
				ServiceEventLog.MachineName = "."; // Local machine
				ServiceEventLog.EnableRaisingEvents = true;

				// Write to Log (just to prove its working)
				Logging.WriteToLog(this, this.ServiceName + " " + "configured to use this Event Log");


			}
			catch (Exception ex)
			{
				ServiceEventLog.Log = "";
				Logging.WriteToLog(this, new Exception("Unable to complete setup of custom Event Log for " + ServiceName + " " + "Properties" + Environment.NewLine + ex.Message));
			}
		}


		#endregion

	}
}
