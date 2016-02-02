#region Using

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BusinessObjects.WorkManagement
{
	#region Worker class

	public partial class Worker
	{
		#region Overrides

		public override string ToString()
        {
			try
			{
				if(mstrForenames != null && !string.Empty.Equals(mstrForenames.Trim()))
				{
					if(mstrSurname != null && !string.Empty.Equals(mstrSurname.Trim()))
					{
						return (mstrForenames.Trim() + " " + mstrSurname).Trim();
					}
					return mstrForenames.Trim();
				}
				else if(mstrSurname != null && !string.Empty.Equals(mstrSurname.Trim()))
				{
                    return mstrSurname.Trim();

				}
				return "Unknown Worker";
			}
			catch(Exception exception)
			{
				return "Unknown Worker";
			}
		}

		#endregion

        #region Public Properties

        public string DisplayName
        {
            get
            {
                return this.ToString();
            }
        }

        public string SortedPrimaryArea
        {
            get
            {
                string lReturn = string.Empty;
                if (this.AreaDetails != null
                    && this.AreaDetails.PrimaryArea != null
                    && !string.IsNullOrEmpty(this.AreaDetails.PrimaryArea.Name))
                {
                    lReturn = this.AreaDetails.PrimaryArea.Name;
                }
                lReturn += ":" + DisplayName;
                return lReturn;
            }
        }

        public string SortedSubArea
        {
            get
            {
                string lReturn = string.Empty;
                if (this.AreaDetails != null
                    && this.AreaDetails.SubArea != null
                    && !string.IsNullOrEmpty(this.AreaDetails.SubArea.Name))
                {
                    lReturn = this.AreaDetails.SubArea.Name;
                }
                lReturn += ":" + DisplayName;
                return lReturn;
            }
        }

        #endregion
    }

	#endregion
}
