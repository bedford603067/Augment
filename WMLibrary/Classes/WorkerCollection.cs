using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
	public partial class WorkerCollection
	{
		public Worker Find(string loginName)
		{
			if(loginName != null)
			{
				foreach(Worker worker in this)
				{
					if(worker.LoginName.ToLower().Trim().Equals(loginName.ToLower().Trim()))
					{
						return worker;
					}
				}
			}
			return null;
		}
	}
}
