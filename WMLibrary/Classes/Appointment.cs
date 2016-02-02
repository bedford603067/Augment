using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class Appointment
    {
        #region Derived Properties

        public bool HasAttendanceOutcomeBeenRecorded
        {
            get
            {
                // One or other will be set if User has visited the Property
                return (mblnWasAttended || mblnWasMissed);
            }
        }

        public bool IsGSSPaymentRequired
        {
            get
            {
                if (mobjSchedulerReasonMissed != eSchedulerAppointmentMissedReason.DefaultValue)
                {
                    if (mobjSchedulerReasonMissed.ToString().Substring(mobjSchedulerReasonMissed.ToString().Length - 2, 2) == "_Y")
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool WasCancelledByCustomer
        {
            get
            {
                return (mobjSchedulerReasonMissed == eSchedulerAppointmentMissedReason.Cancelled_by_customer_N);
            }
        }

        #endregion

        #region Public Methods

        public static string ExtractTimeFromDate(DateTime inputDate)
        {
            return inputDate.TimeOfDay.ToString().Substring(0, 5);
        }

        public static DateTime IncludeTimeInDate(DateTime inputDate, string time)
        {
            inputDate = new DateTime(inputDate.Year,
                                inputDate.Month,
                                inputDate.Day,
                                int.Parse(time.Substring(0, time.IndexOf(":"))),
                                int.Parse(time.Substring(time.IndexOf(":") + 1)),
                                0);

            return inputDate;
        }

        #endregion
    }
}
