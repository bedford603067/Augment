using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class SkillsMatrix
    {
        public SkillsMatrix(int noOfResourcesRequired)
        {
            int resourceIndex = 0;

            mintNoOfResourcesRequired = noOfResourcesRequired;
            mColResourceProfiles = new ResourceProfileCollection();
            while (resourceIndex < noOfResourcesRequired)
            {
                mColResourceProfiles.Add(new ResourceProfile());
                mColResourceProfiles[resourceIndex].Skills = new SkillCollection();
                resourceIndex++;
            }
        }
    }
}
