using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class ActivityTask
    {
        [System.Xml.Serialization.XmlIgnore]
        public SkillCollection SkillsRequired
        {
            get
            {
                if(this.mobjSkillsBreakdown==null || this.mobjSkillsBreakdown.ResourceProfiles == null || this.mobjSkillsBreakdown.ResourceProfiles.Count < 1)
                {
                    return null;
                }

                return mobjSkillsBreakdown.ResourceProfiles[0].Skills;
            }
        }

        public class SearchByDescriptionClass
        {
            string _description;

            public SearchByDescriptionClass(string description)
            {
                this._description = description;
            }

            public bool PredicateDelegate(ActivityTask memberObject)
            {
                return memberObject.Description.ToUpper() == _description.ToUpper();
            }
        }
    }
}
