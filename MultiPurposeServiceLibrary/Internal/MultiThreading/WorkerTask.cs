using System;
using System.Configuration;

using MultiPurposeService.Public.MultiThreading;

namespace MultiPurposeService.Internal.MultiThreading
{
    public class WorkerTask:MultiThreadedClass
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
			return state;
		}

		public void InvokeHandleException(MultiThreadedClass sender, Exception childException)
		{
			throw childException;
		}

		#endregion

		#region Private Fields

		private WorkerTaskInfo mobjTaskInfo = null;

		#endregion

		#region Public Methods

		public WorkerTask(WorkerTaskInfo taskInfo)
        {
			mobjTaskInfo = taskInfo;
        }

        public delegate void DoWorkPointer();

		static readonly object lockingObject = new object();
		public void DoWork()
        {
            WorkerTaskInfo objThreadTaskInfo = new WorkerTaskInfo();
			AssemblyContainer objExecutor = null;
            int intBatchCounter = 0;

            // Create TaskInfo instance for use by each individual Batch
            objThreadTaskInfo = new WorkerTaskInfo();
            // Common to all Batches
            objThreadTaskInfo.MethodInformation = mobjTaskInfo.MethodInformation;
            // Starting Range and per Batch ItemCount
			objThreadTaskInfo.BatchSize = mobjTaskInfo.BatchSize;
			objThreadTaskInfo.BatchInformation.StartOfRange = mobjTaskInfo.BatchInformation.StartOfRange;
            objThreadTaskInfo.BatchInformation.EndOfRange = mobjTaskInfo.BatchInformation.StartOfRange + mobjTaskInfo.BatchSize;

            while (intBatchCounter < mobjTaskInfo.NoOfBatchesRequired)
            {
				// Perform Work
				if (HasStopWorkRequestBeenReceived == true)
				{
					IsBusy = false;
					OnCallback(this, "Thread received Stop request (BatchCounter=" + intBatchCounter.ToString() + ")");
					return;
				}
				// Logging (Optional)
				if (ConfigurationManager.AppSettings.Get("LoggingMode") == "verbose")
				{
					Logging.WriteToLog(this, "Commencing Batch {" + intBatchCounter.ToString() + "}" + " " + "Start of Range = " + mobjTaskInfo.BatchInformation.StartOfRange.ToString());
				}
				try
				{
					// Dynamically Invoke Method for current Batch
					objExecutor = new AssemblyContainer();
                    objExecutor.ExecuteMethod(objThreadTaskInfo);
                    // Prepare to Invoke Method for next Batch
                    objThreadTaskInfo.BatchInformation.StartOfRange += (objThreadTaskInfo.BatchSize + 1); // + 1 assumes a BETWEEN query in SQL
                    objThreadTaskInfo.BatchInformation.EndOfRange = objThreadTaskInfo.BatchInformation.StartOfRange + objThreadTaskInfo.BatchSize;
                    intBatchCounter += 1; // Increment Batch Counter
				}
				catch (Exception excE)
				{
					OnException(this, excE);
					return;
				}
				finally
				{
					//System.Threading.Monitor.Exit(objExecutor);
				}
			}

			// End Work
			IsBusy = false;
			OnCallback(this, "Thread completed its work (BatchCounter=" + intBatchCounter.ToString() + ")");
		}

		#endregion
	}
}
