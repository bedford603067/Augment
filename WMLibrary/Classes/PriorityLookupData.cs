using System;
using System.Collections.Generic;
using System.Text;

using System.Data;

namespace BusinessObjects.WorkManagement
{
    public partial class PriorityLookupData
    {
        public static BusinessObjects.WorkManagement.PriorityLookupData Populate(DataSet dsResults)
        {
            BusinessObjects.WorkManagement.PriorityLookupData lookupData = new BusinessObjects.WorkManagement.PriorityLookupData();

            // Scheduling Priority
            if (dsResults.Tables[0].Rows.Count > 0)
            {
                lookupData.Scheduling = SchedulingPriorityCollection.Populate(dsResults.Tables[0].Select());
            }

            // Dispatch Priority
            if (dsResults.Tables[1].Rows.Count > 0)
            {
                lookupData.Dispatch = new DispatchPriorityCollection();
                for (int intIndex = 0; intIndex < dsResults.Tables[1].Rows.Count; intIndex++)
                {
                    lookupData.Dispatch.Add(new DispatchPriority());
                    lookupData.Dispatch[intIndex].ID = (int)dsResults.Tables[1].Rows[intIndex]["PriorityID"];
                    lookupData.Dispatch[intIndex].Description = dsResults.Tables[1].Rows[intIndex]["PriorityDesc"].ToString();
                    lookupData.Dispatch[intIndex].Rank = (int)dsResults.Tables[1].Rows[intIndex]["NumericRank"];
                }
            }

            return lookupData;
        }
    }
}
