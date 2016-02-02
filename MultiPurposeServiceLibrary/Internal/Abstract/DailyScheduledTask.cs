using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;

using MultiPurposeService.Interfaces;

namespace MultiPurposeService.Internal.Abstract
{
    public class DailyScheduledTask:PollingThread
    {
        #region Private Fields

        private string _startTimeAppSettingName = "DailyScheduledTask_StartTimeHourOfTheDay";
        private string _daysOfTheWeekAppSettingName = "DailyScheduledTask_DaysOfTheWeek";

        #endregion

        #region Private Properties

        private int TaskIntervalInSeconds
        {
            get
            {
                //  We need a consistent value for the time this method starts
                DateTime currentDateTime = DateTime.Now;
                string[] daysOfTheWeek = null;

                //  Default is to start at 2am
                int startTimeHourOfTheDay = 2;  
                int startTimeMinutesOfTheDay = 0;

                //  Unless we have some config setting to say otherwise
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[_startTimeAppSettingName]))
                {
                    string startTimeOfTheDay = ConfigurationManager.AppSettings[_startTimeAppSettingName];
                    if (startTimeOfTheDay.IndexOf(":") > 0)
                    {
                        string[] timeArgs = startTimeOfTheDay.Split(new char[] { ':' });
                        startTimeHourOfTheDay = int.Parse(timeArgs[0]);
                        startTimeMinutesOfTheDay = int.Parse(timeArgs[1]);
                    }
                    else
                    {
                        startTimeHourOfTheDay = int.Parse(startTimeOfTheDay);
                    }
                }

                //  Work out when we ought to start the task
                DateTime nextStartDateTime = new DateTime(currentDateTime.Year,
                                                          currentDateTime.Month,
                                                          currentDateTime.Day,
                                                          startTimeHourOfTheDay, 
                                                          startTimeMinutesOfTheDay, 
                                                          0);

                //  Check if we are currently after the Start Time...
                if (currentDateTime > nextStartDateTime)
                {
                    //  if so, we need to roll the start into the following day
                    nextStartDateTime = nextStartDateTime.AddDays(1);
                }

                // Adjust if Task configured to only run on certain Day(s) of the Week
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[_daysOfTheWeekAppSettingName]))
                {
                    bool dueToRunOnComputedStartDate = false;

                    daysOfTheWeek = ConfigurationManager.AppSettings[_daysOfTheWeekAppSettingName].Split(new char[] {','});
                    for (int dayIndex = 0; dayIndex < daysOfTheWeek.Length; dayIndex++)
                    {
                        if (nextStartDateTime.DayOfWeek == (DayOfWeek)Enum.Parse(typeof(DayOfWeek), daysOfTheWeek[dayIndex]))
                        {
                            // OK to run on Day that NextStartDate falls upon
                            dueToRunOnComputedStartDate = true;
                            break;
                        }
                    }

                    while (!dueToRunOnComputedStartDate)
                    {
                        // Keep adding days till NextStartDate falls on an OK DayOfTheWeek
                        nextStartDateTime =  nextStartDateTime.AddDays(1);
                        for (int dayIndex = 0; dayIndex < daysOfTheWeek.Length; dayIndex++)
                        {
                            if (nextStartDateTime.DayOfWeek == (DayOfWeek)Enum.Parse(typeof(DayOfWeek), daysOfTheWeek[dayIndex]))
                            {
                                dueToRunOnComputedStartDate = true;
                                break;
                            }
                        }
                    }
                }

                //  We want to return a number of seconds from now at which the task should start
                TimeSpan timeSpanUntilTaskRuns = nextStartDateTime.Subtract(currentDateTime);
                int totalSeconds = (int)timeSpanUntilTaskRuns.TotalSeconds;
                if (totalSeconds < 1)
                {
                    totalSeconds = 86400;   //  Number of seconds in 1 day
                }

                return totalSeconds;
            }
        }

        #endregion

        #region Public Constructors

        public DailyScheduledTask(IProcessor processorInstance, string taskName):
            base(processorInstance)
        {
            _startTimeAppSettingName = taskName + "_StartTimeHourOfTheDay";
            _daysOfTheWeekAppSettingName = taskName + "_DaysOfTheWeek";

            ResetInterval(this.TaskIntervalInSeconds);
            this.ProcessorRan += new ProcessorRanHandler(DailyScheduledTask_ProcessorRan);
        }

        void DailyScheduledTask_ProcessorRan()
        {
            // Recompute Interval (else Service restarts will affect frequency incorrectly)
            ResetInterval(this.TaskIntervalInSeconds);

            //System.Diagnostics.Debug.Print(DateTime.Now.ToString() + " DailyScheduledTask_ProcessorRan:" + _startTimeAppSettingName + ":" + this.Interval.ToString());
        }

        #endregion
    }
}
