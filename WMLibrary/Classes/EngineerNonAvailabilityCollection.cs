using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    public partial class EngineerNonAvailabilityCollection
    {

        #region Public Methods

        public EngineerNonAvailability FindByTrentID(string systemID)
        {
            if (!string.IsNullOrEmpty(systemID))
            {
                foreach (EngineerNonAvailability objNA in this)
                {
                    if (objNA.SystemID.ToLower().Trim().Equals(systemID.ToLower().Trim()))
                    {
                        return objNA;
                    }
                }
            }
            return null;
        }

        public EngineerNonAvailability FindByClickID(int systemID)
        {
            if (systemID > 0)
            {
                foreach (EngineerNonAvailability objNA in this)
                {
                    if (objNA.ID == systemID.ToString())
                    {
                        return objNA;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
