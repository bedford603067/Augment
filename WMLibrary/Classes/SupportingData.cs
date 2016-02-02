using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class SupportingDataRequestCollection : System.Collections.Generic.List<eWMSourceSystem>
    {
    }

    #region Class AllocatedWorkSupportingData

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute("AllocatedWorkSupportingData", Namespace = "http://FinalBuild.co.uk/BusinessObjects.WorkManagement")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://FinalBuild.co.uk/BusinessObjects.WorkManagement", IsNullable = false)]
	[System.Runtime.Serialization.DataContractAttribute()]
    public class AllocatedWorkSupportingData
    {
		private eWMSourceSystem _sourceSystem;
        private SupportingAssignmentDetailsCollection _assignments;

		[System.Runtime.Serialization.DataMemberAttribute()]
		public eWMSourceSystem SourceSystem
        {
            get { return this._sourceSystem; }
            set { if (!this._sourceSystem.Equals(value)) { this._sourceSystem = value; };}
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public SupportingAssignmentDetailsCollection Assignments
        {
            get { return this._assignments;}
            set { this._assignments = value;}
        }

    }

    #endregion

    #region Class AllocatedWorkSupportingDataCollection

	[System.Runtime.Serialization.CollectionDataContractAttribute()]
	public class AllocatedWorkSupportingDataCollection : BaseBindingList<AllocatedWorkSupportingData>
    {

    }

    #endregion

}
