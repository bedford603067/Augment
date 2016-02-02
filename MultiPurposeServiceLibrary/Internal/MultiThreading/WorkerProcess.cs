using System;
using System.Collections;
using System.Threading;
using System.Configuration;

using MultiPurposeService.Public.MultiThreading;

namespace MultiPurposeService.Internal.MultiThreading
{
	internal class WorkerProcess : MultiThreadedClass
	{
		#region Inter-Thread Communication

		public delegate object ParentCallback(MultiThreadedClass sender, object state);
		public delegate void HandleException(MultiThreadedClass sender, Exception childException);

		public event ParentCallback OnCallback;
		public event HandleException OnException;

		void InitialiseDelegates()
		{
			OnCallback += new ParentCallback(InvokeParentCallback);
			OnException += new HandleException(InvokeHandleException);
		}

		public object InvokeParentCallback(MultiThreadedClass sender, object state)
		{
			Logging.WriteToLog(this,state.ToString());

			if (sender.IsBusy == false)
			{
				int intNonBusyThreads = 0;
				foreach (WorkerTask workerTask in mcolTasks)
				{
					if (workerTask.IsBusy == false)
					{
						intNonBusyThreads += 1;
					}
				}
				if (intNonBusyThreads == mcolTasks.Count)
				{
					IsBusy = false;
				}
			}

			// Expose Callback to any callers who may be wired in to this event as well
			if (OnCallback != null)
			{
				OnCallback(sender, state);
			}

			return (object)true;
		}

		public void InvokeHandleException(MultiThreadedClass sender, Exception childException)
		{
			// Event Log
			Logging.WriteToLog(this, childException);

			// Expose error to any callers who may be wired in to this event as well
			if (OnException != null)
			{
				OnException(sender, childException);
			}
		}

		#endregion

		#region Delegates and Events

		public delegate void ProgressHandler(string progressMessage);
		public event ProgressHandler ProgressMessageRaised;

		public delegate bool StartProcessPointer(string taskName);

		#endregion

		#region Private Fields

		private WorkerTaskInfo mobjTaskInfo = null;
		private ArrayList mcolTasks = new ArrayList();

		#endregion

		#region Public Methods

		public WorkerProcess(WorkerTaskInfo taskInfo)
		{
			mobjTaskInfo=taskInfo;
		}

		public bool Start()
		{
			if (mobjTaskInfo == null)
			{
				throw new Exception("No Task Information provided for Worker Process to use");
			}

			RunTasks();
			StartTimer();

			return true;
		}

		public void Stop()
		{
			ArrayList colExceptions = new ArrayList();
			if (mcolTasks != null)
			{
				foreach (WorkerTask objThread in mcolTasks)
				{
					try
					{
						objThread.ReceiveStopRequest(this);
					}
					catch (Exception excE)
					{
						colExceptions.Add(excE);
					}
				}
			}
			if (colExceptions.Count > 0)
			{
				throw (Exception)colExceptions[0];
			}
			IsBusy = false;
		}

		#endregion

		#region Private Methods

		static readonly object lockingObject = new object();
		public void RunTasks()
		{
			WorkerTaskInfo objThreadTaskInfo = null;
			WorkerTask workerTask = null;
			int intNoOfBatchesPerThread = 1;    // 1 - If no Range Info is single call
            int intEstimatedNoOfItemsPerThread = 1; // 1 - If no Range Info is single call

			if (mobjTaskInfo.BatchInformation.ItemCount <= mobjTaskInfo.BatchSize)
			{
				mobjTaskInfo.NoOfThreadsToUse = 1;
			}
			else
			{
				mobjTaskInfo.NoOfBatchesRequired = (mobjTaskInfo.BatchInformation.ItemCount / mobjTaskInfo.BatchSize) + 1;
				intNoOfBatchesPerThread = (mobjTaskInfo.NoOfBatchesRequired / mobjTaskInfo.NoOfThreadsToUse) + 1;
				intEstimatedNoOfItemsPerThread = intNoOfBatchesPerThread * mobjTaskInfo.BatchSize;
			}

			for (int intIndex = 0; intIndex < mobjTaskInfo.NoOfThreadsToUse; intIndex++)
			{
				try
				{
                    //lock (lockingObject)
                    //{
						// Create TaskInfo for individual Thread to process
						objThreadTaskInfo = new WorkerTaskInfo();
						// Common to all Threads
						objThreadTaskInfo.MethodInformation = mobjTaskInfo.MethodInformation;
						objThreadTaskInfo.BatchSize = mobjTaskInfo.BatchSize;
						objThreadTaskInfo.NoOfBatchesRequired = intNoOfBatchesPerThread;
						// Thread specific
						objThreadTaskInfo.BatchInformation.ItemCount = intEstimatedNoOfItemsPerThread;
						objThreadTaskInfo.BatchInformation.StartOfRange = mobjTaskInfo.BatchInformation.StartOfRange;
						objThreadTaskInfo.BatchInformation.EndOfRange = mobjTaskInfo.BatchInformation.StartOfRange + intEstimatedNoOfItemsPerThread;
						// Create new Task on own Asynch Thread to perform the process
						workerTask = new WorkerTask(objThreadTaskInfo);
						WireEvents(workerTask);
						mcolTasks.Add(workerTask);
						WorkerTask.DoWorkPointer objMethodPointer = new WorkerTask.DoWorkPointer(workerTask.DoWork);
						// Start Task
						objMethodPointer.BeginInvoke(null, null);
					//}
				}
				catch (Exception excE)
				{
					OnException(this, excE);
				}
				finally
				{
					//// Unlock the main TaskInfo instances
					//Monitor.Exit(mobjTaskInfo);
				}
				// Logging (Optional)
				if (ConfigurationManager.AppSettings.Get("LoggingMode") == "verbose")
				{
					Logging.WriteToLog(this, "Worker Process started on Thread{" + intIndex.ToString() + "}" + " " + "Batches Per Thread=" + intNoOfBatchesPerThread.ToString() + " " + "Batch Size " + mobjTaskInfo.BatchSize.ToString());
				}
				// Increment Range Information that will be used for next Task required (if NoThreadsToUse > 1)
				mobjTaskInfo.BatchInformation.StartOfRange += (intEstimatedNoOfItemsPerThread + 1); // + 1 allows for BETWEEN query in SQL
			}
		}
		
		//public void RunTasks()
		//{
		//    WorkerTaskInfo objThreadTaskInfo = null;
		//    WorkerTask workerTask = null;
		//    Thread processingThread = null;
		//    ArrayList colWorkerThreads = new ArrayList();
		//    int intNoOfBatchesPerThread = 0;
		//    int intEstimatedNoOfItemsPerThread = 0;

		//    if (mobjTaskInfo.BatchInformation.ItemCount <= mobjTaskInfo.BatchSize)
		//    {
		//        mobjTaskInfo.NoOfThreadsToUse = 1;
		//    }
		//    else
		//    {
		//        mobjTaskInfo.NoOfBatchesRequired = (mobjTaskInfo.BatchInformation.ItemCount / mobjTaskInfo.BatchSize) + 1;
		//        intNoOfBatchesPerThread = (mobjTaskInfo.NoOfBatchesRequired / mobjTaskInfo.NoOfThreadsToUse) + 1;
		//        intEstimatedNoOfItemsPerThread = intNoOfBatchesPerThread * mobjTaskInfo.BatchSize;
		//    }

		//    for (int intIndex = 0; intIndex < mobjTaskInfo.NoOfThreadsToUse; intIndex++)
		//    {
		//        // Create TaskInfo for individual Thread to process
		//        objThreadTaskInfo = new WorkerTaskInfo();
		//        // Common to all Threads
		//        objThreadTaskInfo.MethodInformation = mobjTaskInfo.MethodInformation;
		//        objThreadTaskInfo.BatchSize = mobjTaskInfo.BatchSize;
		//        objThreadTaskInfo.NoOfBatchesRequired = intNoOfBatchesPerThread;
		//        // Thread specific
		//        objThreadTaskInfo.BatchInformation.ItemCount = intEstimatedNoOfItemsPerThread;
		//        objThreadTaskInfo.BatchInformation.StartOfRange = mobjTaskInfo.BatchInformation.StartOfRange;
		//        objThreadTaskInfo.BatchInformation.EndOfRange = mobjTaskInfo.BatchInformation.StartOfRange + intEstimatedNoOfItemsPerThread;
		//        // Create new Task on own Thread to perform the process
		//        workerTask = new WorkerTask(objThreadTaskInfo);
		//        WireEvents(workerTask);
		//        mcolTasks.Add(workerTask);
		//        processingThread = new Thread(new ThreadStart(workerTask.DoWork));
		//        colWorkerThreads.Add(processingThread);
		//        // Increment Range Information that will be used for next Task required (if NoThreadsToUse > 1)
		//        mobjTaskInfo.BatchInformation.StartOfRange += (intEstimatedNoOfItemsPerThread + 1); // + 1 allows for BETWEEN query in SQL
		//    }

		//    // Start the Tasks
		//    int intThreadCounter = 1;
		//    foreach (Thread workerThread in colWorkerThreads)
		//    {
		//        workerThread.Start();
		//        // Logging (Optional)
		//        if (ConfigurationManager.AppSettings.Get("LoggingMode") == "verbose")
		//        {
		//            Logging.WriteToLog(this, "Worker Process started on Thread{" + intThreadCounter.ToString() + "}" + " " + "Batches Per Thread=" + intNoOfBatchesPerThread.ToString() + " " + "Batch Size " + mobjTaskInfo.BatchSize.ToString());
		//            intThreadCounter += 1;
		//        }
		//        IsBusy = true; // Must be set as soon as one Task has been started
		//    }

		//}

		private void WireEvents(WorkerTask workerTask)
		{
			// Wire Server instance to call Service copy of method for OnCallback
			workerTask.OnCallback -= new WorkerTask.ParentCallback(workerTask.InvokeParentCallback);
			workerTask.OnCallback += new WorkerTask.ParentCallback(this.InvokeParentCallback);

			// Wire child Server class to Pass Exceptions up to Service instance 
			workerTask.OnException -= new WorkerTask.HandleException(workerTask.InvokeHandleException);
			workerTask.OnException += new WorkerTask.HandleException(this.InvokeHandleException);
		}

		#endregion

		#region Timer

		private System.Timers.Timer ServiceTimer;
		private int mintTimerInterval = 5000; // 5 seconds

		private void StartTimer()
		{
			// ServiceTimer
			if (ConfigurationManager.AppSettings.Get("TimerInterval") != null)
			{
                mintTimerInterval = int.Parse(ConfigurationManager.AppSettings.Get("TimerInterval"));
                Logging.WriteToLog(this, this.ServiceName + " " + "Timer Interval set to " + ConfigurationManager.AppSettings.Get("TimerInterval"));
			}
			ServiceTimer = new System.Timers.Timer();
			ServiceTimer.Interval = mintTimerInterval;
			ServiceTimer.Start();
			Logging.WriteToLog(this,this.ServiceName + " " + "Started");
		}

		public void ServiceTimer_TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// If no longer busy stopping the timer will stop the current thread the instance is executing on
			ServiceTimer.Enabled = IsBusy;
		}

		#endregion
	}
}
