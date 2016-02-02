using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    [Serializable]
    public class JobLoadException : Exception
    {
        public int JobID;
        public eWMSourceSystem SourceSystem;

        public JobLoadException(Exception excE):base(excE.Message, excE.InnerException)
        {
            // Pass through Args
        }

        public override string Message
        {
            get
            {
                return string.Format("Job {0} of type {1} could not be loaded", JobID, SourceSystem);
            }
        }
    }
}
