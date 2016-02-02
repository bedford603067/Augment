using System;

namespace MultiPurposeService.Interfaces
{
	/// <summary>
	/// Summary description for IProcessor.
	/// </summary>
	public interface IProcessor
	{
		void Process(object inputData);
		void Shutdown();
	}

    /// <summary>
    /// Summary description for IHttpRequestProcessor.
    /// </summary>
    public interface IHttpRequestProcessor
    {
        object Process(object inputData);
        void Shutdown();
    }

}
