#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BusinessObjects.WorkManagement
{
	#region Location class

	public partial class Location
	{
		#region Private Fields

		private BusinessObjects.WorkManagement.Asset[] _assets = null;

		#endregion

		#region Public Properties

		[System.Xml.Serialization.XmlElement(typeof(BusinessObjects.WorkManagement.Asset[]))]
		[System.Runtime.Serialization.DataMemberAttribute()]
		public BusinessObjects.WorkManagement.Asset[] Assets
		{
			get { return this._assets; }
			set { this._assets = value; }
		}

		#endregion

        #region Public Methods

        public override string ToString()
        {
            string locationString = base.ToString();

            if (this.SiteID > 0)
            {
                locationString = string.Format("Site {0} {1}", this.SiteID.ToString(), this.Description);
            }
            else
            {
                StringBuilder address = new StringBuilder();
                // Post Code
                if (this.PostCode != null && this.PostCode.Trim().Length > 0)
                {
                    address.Append(this.PostCode.Trim());
                    address.Append(", ");
                }
                // Sub Building
                if (this.SubBuilding != null && this.SubBuilding.Trim().Length > 0)
                {
                    address.Append(this.SubBuilding.Trim());
                    address.Append(" ");
                }
                // House No
                if (this.HouseNo > 0)
                {
                    address.Append(this.HouseNo);
                    address.Append(" ");
                }
                // House Name
                if (this.HouseName != null && this.HouseName.Trim().Length > 0)
                {
                    address.Append(this.HouseName.Trim());
                    address.Append(", ");
                }
                //// Org Name
                if (this.OrgName != null && this.OrgName.Trim().Length > 0)
                {
                    address.Append(this.OrgName.Trim());
                    address.Append(", ");
                }
                // Street
                if (this.Street != null && this.Street.Trim().Length > 0)
                {
                    address.Append(this.Street.Trim());
                    address.Append(", ");
                }
                // District
                if (this.District != null && this.District.Trim().Length > 0)
                {
                    address.Append(this.District.Trim());
                    address.Append(", ");
                }
                // Town
                if (this.Town != null && this.Town.Trim().Length > 0)
                {
                    address.Append(this.Town.Trim());
                    address.Append(", ");
                }
                // Set Address string
                locationString = address.ToString();
                
                // Ends with ', ' then lose the comma and space
                if (locationString.EndsWith(", "))
                {
                    locationString = locationString.Substring(0, locationString.Length - 2);
                }
                // If we have no Address string but do have a description, then use it
                if ((locationString == null || locationString.Trim().Length == 0) && this.Description != null && this.Description.Trim().Length > 0)
                {
                    locationString = this.Description.Trim();
                }
            }

            return locationString;
        }

        #endregion
    }

	#endregion
}
