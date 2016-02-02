using System;
using System.Collections;
using System.Text;

namespace MultiPurposeService.Public.MultiThreading
{
	[Serializable]
	public class WorkerTaskInfo
	{
		private MethodInfo mobjMethodInfo;
		private BatchInfo mobjBatchInfo;

		public int NoOfBatchesRequired = 1;
		public int BatchSize = 100;
		public int NoOfThreadsToUse = 1;

		public MethodInfo MethodInformation
		{
			get
			{
				if (mobjMethodInfo == null)
				{
					mobjMethodInfo = new MethodInfo();
				}
				return mobjMethodInfo;
			}
			set
			{
				mobjMethodInfo = value;
			}
		}
		public BatchInfo BatchInformation
		{
			get
			{
				if (mobjBatchInfo == null)
				{
					mobjBatchInfo = new BatchInfo();
				}
				return mobjBatchInfo;
			}
			set
			{
				mobjBatchInfo = value;
			}
		}

		public object[] GetMethodParameters()
		{
			object[] methodParameters = null;

			// Append additional MethodArgs if appropriate
            if (MethodInformation.AppendRangeInfoToArgs == true)
            {
                ArrayList colMethodArgs = new ArrayList();
                if (MethodInformation.MethodArgs != null)
                {
                    for (int intIndex = 0; intIndex < MethodInformation.MethodArgs.Length; intIndex++)
                    {
                        colMethodArgs.Add(MethodInformation.MethodArgs[intIndex]);
                    }
                }
                colMethodArgs.Add(BatchInformation.StartOfRange);
                colMethodArgs.Add(BatchInformation.EndOfRange);
                methodParameters = (object[])colMethodArgs.ToArray(typeof(object));
            }
            else
            {
                methodParameters = MethodInformation.MethodArgs;
            }

			return methodParameters;
		}
	}

	[Serializable]
	public class MethodInfo
	{
		public string TypeName;
		public string MethodName;
		public object[] MethodArgs;
		public bool IsStaticMethod = true;
        public bool AppendRangeInfoToArgs = false;
        public object[] ConstructorArgs; // Where non-static method create instance of TypeName
	}

	[Serializable]
	public class BatchInfo
	{
		public int ItemCount;
		public int StartOfRange;
		public int EndOfRange;
	}


}
