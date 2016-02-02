using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

namespace MultiPurposeService.Internal
{
	public class Logging
	{
		public static void WriteToLog(object sender,string informationMessage)
		{
            informationMessage= sender.ToString() + ":" + " " + informationMessage;
			FinalBuild.LogWriter.WriteToLog(informationMessage);
		}
		
		public static void WriteToLog(object sender,Exception exception)
		{
            FinalBuild.LogWriter.WriteToLog(new Exception(sender.ToString() + ":" + " ",exception));
		}
	}
}
