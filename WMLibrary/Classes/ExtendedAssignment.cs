using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class ExtendedAssignment
    {

        #region Public Classes

        public class SortByDueDate : System.Collections.Generic.IComparer<ExtendedAssignment>
        {
            public int Compare(ExtendedAssignment x, ExtendedAssignment y)
            {
                return x.Assignment.DueDate.CompareTo(y.Assignment.DueDate);
            }
        }


        #endregion
    }
}

