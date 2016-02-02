using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Serialization;

namespace BusinessObjects.WorkManagement
{
    public partial class PerformanceMeasurementGroupCollection
    {
        public PerformanceMeasurementGroup this[string groupName]
        {
            get
            {
                int itemIndex = this.Find("Description", groupName);
                if (itemIndex > -1)
                {
                    return this[itemIndex];
                }

                return null;
            }
        }
    }
    public partial class PerformanceMeasurementCollection
    {
        public PerformanceMeasurement this[string indicatorCode]
        {
            get
            {
                int itemIndex = this.Find("Code", indicatorCode);
                if (itemIndex > -1)
                {
                    return this[itemIndex];
                }

                return null;
            }
        }

        public override string ToString()
        {
            if (this.Count > 0)
            {
                string measurementsToString = string.Empty;

                foreach (PerformanceMeasurement measurement in this)
                {
                    measurementsToString += string.Format("{0} : Value={1};Max={2};Min={3}",
                                                    measurement.Description,
                                                    measurement.Value.ToString().Replace("m", string.Empty)+"*",
                                                    measurement.MaxValue.ToString().Replace("m", string.Empty),
                                                    measurement.MinValue.ToString().Replace("m", string.Empty));
                    measurementsToString += ",";
                }
                measurementsToString = measurementsToString.Substring(0, measurementsToString.Length - 1);

                return measurementsToString;
            }

            return base.ToString();
        }
    }
    
    public partial class PerformanceIndicatorGroupCollection
    {
        public PerformanceIndicatorGroup this[string groupName]
        {
            get
            {
                int itemIndex = this.Find("Description", groupName);
                if (itemIndex > -1)
                {
                    return this[itemIndex];
                }

                return null;
            }
        }
    }
    public partial class PerformanceIndicatorCollection
    {
        public PerformanceIndicator this[string indicatorCode]
        {
            get
            {
                int itemIndex = this.Find("Code", indicatorCode);
                if (itemIndex > -1)
                {
                    return this[itemIndex];
                }

                return null;
            }
        }
    }

    public partial class PerformanceMeasurement
    {
        public class SearchByDescriptionClass
        {
            string _description;

            public SearchByDescriptionClass(string description)
            {
                this._description = description;
            }

            public bool PredicateDelegate(PerformanceMeasurement memberObject)
            {
                return memberObject.Description.ToUpper() == _description.ToUpper();
            }
        }
    }
}
