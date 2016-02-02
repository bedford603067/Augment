using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    public partial class AssignmentConstraints
    {
        private bool _updateInsideTransaction = false;
        public bool UpdateInsideTransaction
        {
            get { return _updateInsideTransaction; }
            set { _updateInsideTransaction = value; }
        }

        private DataAccess _assignmentConstraintsDataAccess; 
        public DataAccess AssignmentConstraintsDataAccess
        {
            get { return _assignmentConstraintsDataAccess; }
            set { _assignmentConstraintsDataAccess = value; }
        }

    }
}
