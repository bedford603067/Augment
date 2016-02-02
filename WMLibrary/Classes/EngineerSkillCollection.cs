using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    public partial class EngineerSkillCollection
    {

        #region Public Methods

        public EngineerSkill FindByName(string skillName)
        {
            if (skillName != null)
            {
                foreach (EngineerSkill skill in this)
                {
                    if (skill.Description.ToLower().Trim().Equals(skillName.ToLower().Trim()))
                    {
                        return skill;
                    }
                }
            }
            return null;
        }

        public EngineerSkill Find(int clickKey)
        {
            if (clickKey > 0)
            {
                foreach (EngineerSkill skill in this)
                {
                    if (skill.ClickID == clickKey)
                    {
                        return skill;
                    }
                }
            }
            return null;
        }

        public EngineerSkill Find(string trentSkillID)
        {
            if (trentSkillID != null)
            {
                foreach (EngineerSkill skill in this)
                {
                    if (skill.TrentID.ToLower().Trim().Equals(trentSkillID.ToLower().Trim()))
                    {
                        return skill;
                    }
                }
            }
            return null;
        }


        #endregion
    }
}
