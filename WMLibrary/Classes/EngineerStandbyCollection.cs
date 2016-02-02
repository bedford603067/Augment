using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    public partial class EngineerStandbyCollection
    {

        #region Public Methods

        public EngineerStandby Find(int rotaScheduleID)
        {
            if (rotaScheduleID > 0)
            {
                foreach (EngineerStandby standby in this)
                {
                    if (standby.ID== rotaScheduleID)
                    {
                        return standby;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
