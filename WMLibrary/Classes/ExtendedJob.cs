using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class ExtendedJob
    {
        #region Derived Public Properties

        [System.Xml.Serialization.XmlIgnore]
        public DateTime AssignedDate
        {
            get
            {
                DateTime lReturn = DateTime.MaxValue;
                foreach (AssignmentDetails item in this.GetAssignedInstances())
                {
                    //  Take the lowest non NULL DueDate
                    if (item.DueDate != DateTime.MinValue && item.DueDate < lReturn)
                    {
                        lReturn = item.DueDate;
                    }
                }
                if (lReturn == DateTime.MaxValue)
                {
                    foreach (AssignmentDetails item in this.GetAssignedInstances())
                    {
                        //  Take the lowest non NULL DateAssigned
                        if (item.DateAssigned != DateTime.MinValue && item.DateAssigned < lReturn)
                        {
                            lReturn = item.DateAssigned;
                        }
                    }
                }
                if (lReturn == DateTime.MaxValue)
                {
                    lReturn = DateTime.MinValue;
                }
                return lReturn;
            }
        }


        #endregion

        #region Public Methods

        public AssignmentDetailsCollection GetAssignedInstances()
        {
            AssignmentDetailsCollection lReturn = new AssignmentDetailsCollection();
            if (this.Instances != null)
            {
                foreach (AssignmentDetails item in this.Instances)
                {
                    if (item.Workers != null
                        && item.Workers.Count > 0)
                    {
                        lReturn.Add(item);
                        break;
                    }
                }
            }
            return lReturn;
        }

        #endregion
    }
}
