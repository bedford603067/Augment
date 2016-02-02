using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    public partial class StockItemCollection
    {
        private bool _updateInsideTransaction = false;
        public bool UpdateInsideTransaction
        {
            get { return _updateInsideTransaction; }
            set { _updateInsideTransaction = value; }
        }

        private DataAccess _stockDataAccess;
        public DataAccess StockDataAccess
        {
            get { return _stockDataAccess; }
            set { _stockDataAccess = value; }
        }
    }
}
