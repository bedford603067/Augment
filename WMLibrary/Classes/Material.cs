using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class Material
    {
        public class SearchByDescriptionClass
        {
            string _description;

            public SearchByDescriptionClass(string description)
            {
                this._description = description;
            }

            public bool PredicateDelegate(Material memberObject)
            {
                return memberObject.Description.ToUpper() == _description.ToUpper();
            }
        }
    }
}
