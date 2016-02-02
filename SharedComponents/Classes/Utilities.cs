#region Using 

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace FinalBuild
{
	#region Utilities class

	public static class Utilities
	{
		#region Transaction helpers
		/// <summary>
		/// Single place to set default transaction properties
		/// Creates a Required transaction scope.
		/// With an Isolation Level of ReadCommitted
		/// </summary>
		/// <returns></returns>
		public static System.Transactions.TransactionScope CreateTransactionScope()
		{
			return CreateTransactionScope(System.Transactions.TransactionScopeOption.Required);
		}

		/// <summary>
		/// Create a transaction scope of specified TransactionScope
		/// Sets the isolation level to ReadCommitted
		/// </summary>
		/// <returns></returns>
		public static System.Transactions.TransactionScope CreateTransactionScope(System.Transactions.TransactionScopeOption tranScopeOption)
		{

			System.Transactions.TransactionOptions tranOption = new System.Transactions.TransactionOptions();
			tranOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

			System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope(tranScopeOption, tranOption);
			//Console.WriteLine(string.Format("Transaction Id={0}, DistId={1}", System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier, System.Transactions.Transaction.Current.TransactionInformation.DistributedIdentifier));
			return scope;
		}
		#endregion
	}

	#endregion
}
