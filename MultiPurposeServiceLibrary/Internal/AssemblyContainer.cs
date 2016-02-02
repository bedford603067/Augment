using System;
using System.Collections;
using System.Text;
using System.Configuration;

namespace MultiPurposeService.Internal
{
	using System.Reflection;

	internal class AssemblyContainer
    {
        #region Private Fields

        private Assembly mobjAssembly = null;
		private string mstrTargetNamespace = "BusinessObjects";
		private string mstrAssemblyPath = @"D:\Work\DataTransferHarness\MeterReadingLibrary\bin\Debug\MeterReadingLibrary.DLL";

        #endregion

        #region Public Methods

        public AssemblyContainer(string assemblyPath, string targetNamespace)
		{
			mstrAssemblyPath = assemblyPath;
			mstrTargetNamespace = targetNamespace;
			mobjAssembly = Assembly.LoadFrom(mstrAssemblyPath);
		}

		public AssemblyContainer()
		{
            if (ConfigurationManager.AppSettings["DefaultExternalAssemblyPath"] != null)
            {
                mstrAssemblyPath = ConfigurationManager.AppSettings.Get("DefaultExternalAssemblyPath");
            }
            if (ConfigurationManager.AppSettings["DefaultExternalNamespace"] != null)
            {
                mstrTargetNamespace = ConfigurationManager.AppSettings.Get("DefaultExternalNamespace");
            }
			mobjAssembly = Assembly.LoadFrom(mstrAssemblyPath);
		}

		public Type GetType(string typeName)
		{
			Type businessObject = null;

			if (mobjAssembly == null)
				throw new Exception("Assembly was not loaded successfully" + " " + "Path=" + mstrAssemblyPath);

			businessObject = mobjAssembly.GetType(mstrTargetNamespace + "." + typeName);
			if (businessObject == null)
				throw new Exception("Type was not loaded successfully" + " " + "Type=" + mstrTargetNamespace + "." + typeName);

			return businessObject;
		}

		public object GetInstance(string typeName, object[] constructorArgs)
		{
			object businessObject = null;

			if (mobjAssembly == null)
				throw new Exception("Assembly was not loaded successfully" + " " + "Path=" + mstrAssemblyPath);

			if (constructorArgs == null)
			{
				businessObject = mobjAssembly.CreateInstance(mstrTargetNamespace + "." + typeName);
			}
			else
			{
				businessObject = mobjAssembly.CreateInstance(mstrTargetNamespace + "." + typeName,
									true,
									BindingFlags.Default | BindingFlags.CreateInstance,
									null,
									constructorArgs,
									null,
									null);
			}
			if (businessObject == null)
				throw new Exception("Type was not loaded successfully" + " " + "Type=" + mstrTargetNamespace + "." + typeName);

			return businessObject;
		}

		public delegate object ExecuteMethodPointer(Public.MultiThreading.WorkerTaskInfo taskInfo);

		static readonly object lockingObject = new object();
		public object ExecuteMethod(Public.MultiThreading.WorkerTaskInfo taskInfo)
		{
			Type objectType = null;
            object businessObject = null;
			object[] methodParameters = null;
			object returnObject = null;
			
			try
			{
                //lock (lockingObject)
                //{
                // Get Method Parameters, merges Range Info if indicated by flag to do so
                methodParameters = taskInfo.GetMethodParameters();
                // Get reference to Type that hosts the method
                objectType = GetType(taskInfo.MethodInformation.TypeName);
                if (taskInfo.MethodInformation.IsStaticMethod == false)
                {
                    // Get instance of Type that hosts the method
                    businessObject = GetInstance(taskInfo.MethodInformation.TypeName, taskInfo.MethodInformation.ConstructorArgs);
                }
                // Execute Method
                if (ConfigurationManager.AppSettings.Get("LoggingMode") == "verbose")
                {
                    LogMethodExecution(taskInfo);
                }
                returnObject =  objectType.InvokeMember(
                                taskInfo.MethodInformation.MethodName,
                                BindingFlags.Default | BindingFlags.InvokeMethod,
                                null, 
                                businessObject,
                                methodParameters);
                if (ConfigurationManager.AppSettings.Get("LoggingMode") == "verbose")
                {
                    Logging.WriteToLog(this, "Execution completed " + taskInfo.MethodInformation.TypeName + "." + taskInfo.MethodInformation.MethodName);
                }
				//}
			}
			catch (Exception excE)
			{
				Logging.WriteToLog(this, new Exception("Dynamic invocation of method " + taskInfo.MethodInformation.MethodName + " " + "on type" + " " + taskInfo.MethodInformation.TypeName + " " + "failed" + Environment.NewLine + excE.Message, excE));
			}
			return returnObject;
		}

		public object ExecuteMethod(string typeName, string methodName, params object[] args)
		{
			object returnObject = null;
			Type businessObject = GetType(typeName);

			try
			{
				returnObject = businessObject.InvokeMember(
								methodName,
								BindingFlags.Default | BindingFlags.InvokeMethod,
								null, null,
								args);
			}
			catch (Exception excE)
			{
				throw new Exception("Dynamic invocation of method " + methodName + " " + "on type" + " " + typeName + " " + "failed" + Environment.NewLine + excE.Message, excE);
			}

			return returnObject;
		}

        #endregion

        #region Private Methods

        private void LogMethodExecution(Public.MultiThreading.WorkerTaskInfo taskInfo)
        {
            string strLogMessage = null;

            strLogMessage = "Executing Method " + taskInfo.MethodInformation.TypeName + "." + taskInfo.MethodInformation.MethodName;
            if (taskInfo.MethodInformation.MethodArgs != null)
            {
                strLogMessage += "Method Args {";
                foreach (object methodArg in taskInfo.MethodInformation.MethodArgs)
                {
                    strLogMessage += methodArg.ToString() + ",";
                }
                strLogMessage += "Method Args }";
            }
            if (taskInfo.MethodInformation.AppendRangeInfoToArgs==true)
            {
                strLogMessage += Environment.NewLine + "Start of Range = " + taskInfo.BatchInformation.StartOfRange.ToString() + "," + "End of Range = " + taskInfo.BatchInformation.EndOfRange.ToString();
            }
            Logging.WriteToLog(this, strLogMessage);
        }

        #endregion
    }
}
