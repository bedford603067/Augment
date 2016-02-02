using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class AssignmentDetails
    {
        /// <summary>
        /// Note - this class is for "internal use" and is quite intentionally not exposed over WCF nor serializable as Xml.
        /// </summary>
        public class AssignmentWrapper
        {
            [System.Xml.Serialization.XmlIgnore]
            public AssignmentDetails Assignment;
            [System.Xml.Serialization.XmlIgnore]
            public string JobTypeName;
            [System.Xml.Serialization.XmlIgnore]
            public int SiteID;
        }

        public class SearchAssignmentsByIDClass
        {
            int mintID;

            public SearchAssignmentsByIDClass(int memberID)
            {
                this.mintID = memberID;
            }

            public bool PredicateDelegate(AssignmentDetails memberInstance)
            {
                return memberInstance.ID == mintID;
            }
        }



    }


    public partial class AssignmentDetailsCollection
    {
        /// <summary>
        /// Get list of unique SourceSystem values present in the collection
        /// </summary>
        /// <returns></returns>
        public BusinessObjects.WorkManagement.eWMSourceSystem[] GetSourceSystems()
        {
            BusinessObjects.WorkManagement.eWMSourceSystem[] arrPropertyValues = null;
            System.Collections.ArrayList colPropertyValues = new System.Collections.ArrayList();

            foreach (AssignmentDetails objDetails in this)
            {
                if (colPropertyValues.IndexOf(objDetails.SourceSystem) < 0)
                {
                    colPropertyValues.Add(objDetails.SourceSystem);
                }
            }

            if (colPropertyValues.Count > 0)
            {
                arrPropertyValues = (BusinessObjects.WorkManagement.eWMSourceSystem[])colPropertyValues.ToArray(typeof(BusinessObjects.WorkManagement.eWMSourceSystem));
            }

            return arrPropertyValues;
        }


        public static List<AssignmentDetails> FindAssignmentsBySourceSystem(eWMSourceSystem SourceSystem, List<AssignmentDetails> searchList)
        {
            return searchList.FindAll(new Predicate<AssignmentDetails>
            (
                new SearchAssignmentsBySourceSystemClass(SourceSystem).PredicateDelegate)
            );
        }

        public class SearchAssignmentsBySourceSystemClass
        {
            eWMSourceSystem mstrSourceSystem;

            public SearchAssignmentsBySourceSystemClass(eWMSourceSystem sourceSystem)
            {
                this.mstrSourceSystem = sourceSystem;
            }

            public bool PredicateDelegate(AssignmentDetails memberInstance)
            {
                return memberInstance.SourceSystem == mstrSourceSystem;
            }
        }

    }
}
