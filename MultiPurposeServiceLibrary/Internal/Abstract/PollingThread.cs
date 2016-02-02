using System;
using System.Threading;

using MultiPurposeService.Public.Abstract;
using MultiPurposeService.Interfaces;

namespace MultiPurposeService.Internal.Abstract
{
	/// <summary>
	/// Summary description for PollingThread.
	public class PollingThread : Base, IServiceContainer
	{
        #region Inter-Thread Communication

        public delegate void HandleException(IProcessor sender, Exception childException);
        public event HandleException OnException;

        #endregion

        #region Internal Events (for use by Inheritors)

        internal delegate void ProcessorRanHandler();
        internal event ProcessorRanHandler ProcessorRan;

        #endregion

        #region Private Fields

        private IProcessor mobjProcessor = null;
		private Thread mobjPollingThread = null;
		private System.Timers.Timer mobjTimer = null;
		private System.TimeSpan mobjPollingInterval;

		private string mstrThreadName = "PollingThread";
		private int mintIntervalInSeconds = 60;

        private bool mblnRunProcessorOnWeekends = true;
        private DateTime[] marrDatesNotToRunProcessor = null;

		#endregion

        #region Private Properties

        private bool OKToRunProcessorAtThisTime
        {
            get
            {
                bool okToRunProcessorAtThisTime = true;

                if (!mblnRunProcessorOnWeekends)
                {
                    switch (DateTime.Now.DayOfWeek)
                    {
                        case DayOfWeek.Saturday:
                        case DayOfWeek.Sunday:
                            {
                                okToRunProcessorAtThisTime = false;
                                break;
                            }
                    } 
                }

                if (okToRunProcessorAtThisTime && marrDatesNotToRunProcessor != null)
                {
                    for (int index = 0; index < marrDatesNotToRunProcessor.Length; index++)
                    {
                        if (marrDatesNotToRunProcessor[index].Date == DateTime.Now.Date)
                        {
                            okToRunProcessorAtThisTime = false;
                            break;
                        }
                    }
                }

                return okToRunProcessorAtThisTime;
            }
        }

        #endregion

        #region Public Properties

        public string Name
		{
			get
			{
				return mstrThreadName;
			}
			set
			{
				mstrThreadName=value;
			}
		}

		public int Interval
		{
			get
			{
				return mintIntervalInSeconds;
			}
		}

        public bool RunProcessorOnWeekends
        {
            get
            {
                return mblnRunProcessorOnWeekends;
            }
            set
            {
                mblnRunProcessorOnWeekends = value;
            }
        }

        public DateTime[] DatesNotToRunProcessor
        {
            get
            {
                return marrDatesNotToRunProcessor;
            }
            set
            {
                marrDatesNotToRunProcessor = value;
            }
        }

        public IProcessor ProcessorInstance
        {
            get
            {
                return mobjProcessor;
            }
        }

        #endregion

		#region Constructors

		public PollingThread(IProcessor processorInstance,int intervalInSeconds,string threadName)
		{
			mobjProcessor=processorInstance;
			mintIntervalInSeconds=intervalInSeconds;
			mstrThreadName=threadName;

		}

		public PollingThread(IProcessor processorInstance,int intervalInSeconds)
		{
			mobjProcessor=processorInstance;
			mintIntervalInSeconds=intervalInSeconds;
		}

		public PollingThread(IProcessor processorInstance)
		{
			mobjProcessor=processorInstance;
		}

		#endregion

		#region Public Methods

		public void Start() 
		{
			CreateThread();
		}

		public void Shutdown()
		{
			Dispose();
        }

        #endregion

        #region Internal Methods (for use by Inheritors)

        internal void ResetInterval(int intervalInSeconds)
        {
            if (mobjTimer != null)
            {
                mobjTimer.Enabled = false;
            }
            mintIntervalInSeconds = intervalInSeconds;
            //System.Diagnostics.Debug.Print(DateTime.Now.ToString() + " ResetInterval:" + this.mintIntervalInSeconds.ToString());
            if (mobjTimer != null)
            {
                mobjTimer.Interval = mintIntervalInSeconds * 1000;  //  Interval is in milliseconds
                //System.Diagnostics.Debug.Print(DateTime.Now.ToString() + " ResetInterval:" + this.mintIntervalInSeconds.ToString() + ":Timer.Interval:" + mobjTimer.Interval.ToString());
                mobjTimer.Enabled = true;
            }
        }

		#endregion

		#region Private Methods

        private void CreateThread()
		{
            try
            {
                mobjPollingThread = new Thread(new ThreadStart(Poll));
                mobjPollingInterval = new System.TimeSpan(0, 0, 0, mintIntervalInSeconds);

                // Run on a background thread
                mobjPollingThread.Name = mstrThreadName;
                mobjPollingThread.IsBackground = true;
                mobjPollingThread.Start();
            }
            catch (Exception excE)
            {
                OnException(mobjProcessor, excE);
            }
		}

		private void Poll()
		{
			mobjTimer = new System.Timers.Timer(mobjPollingInterval.TotalMilliseconds);
			mobjTimer.Elapsed +=new System.Timers.ElapsedEventHandler(mobjTimer_Elapsed); 
			mobjTimer.Enabled=true;
		}

        private void mobjTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                mobjTimer.Enabled = false;
                if (mobjProcessor != null && OKToRunProcessorAtThisTime)
                {
                    mobjProcessor.Process(null);
                    if (ProcessorRan != null)
                    {
                        // Run post Processor event. This may e.g be resetting the interval of the Timer...
                        ProcessorRan();
                    }
                }
            }
            catch (Exception excE)
            {
                OnException(mobjProcessor, excE);
                if (ProcessorRan != null)
                {
                    // Run post Processor event. This may e.g be resetting the interval of the Timer...
                    ProcessorRan();
                }
            }
            finally
            {
                //  We need to re-enable the timer
                mobjTimer.Enabled = true;
            }
        }

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing) 
		{
            try
            {
                if (disposing)
                {
                    // Release managed resources.
                }

                // Release local resources.
                if (mobjProcessor != null)
                {
                    mobjProcessor.Shutdown();
                    mobjProcessor = null;
                }

                // Polling Thread disposal
                if (mobjPollingThread != null)
                {
                    mobjPollingThread.Abort();
                    mobjPollingThread = null;
                }

                // Call Dispose on Base
                base.Dispose(disposing);
            }
            catch(Exception excE)
            {
                OnException(mobjProcessor, excE);
            }
		}

		#endregion
	}
}
