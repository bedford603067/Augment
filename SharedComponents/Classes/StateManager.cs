#region Using

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;

#endregion

namespace FinalBuild
{
	#region StateManager class

	public static class StateManager
	{
		internal static System.Web.Caching.Cache _cache = System.Web.HttpRuntime.Cache;

		#region Constants

		public const int DefaultCacheHours = 4032;	// 7 Days

		#endregion

		#region Public Methods

		#region Public Static Methods

		public static object FetchState(string stateObjectName)
		{
			if (_cache == null)
			{
				return null;
			}
			return _cache[stateObjectName];
		}

		public static void SaveState(string stateObjectName, object instance)
		{
			SaveState(stateObjectName, instance, DefaultCacheHours);
		}

		public static void SaveState(string stateObjectName, object instance, int cacheHours)
		{
			if (_cache == null)
			{
				return;
			}
			ClearState(stateObjectName);
			_cache.Add(stateObjectName, instance, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(cacheHours), System.Web.Caching.CacheItemPriority.NotRemovable, null);
		}

		public static void ClearState(string stateObjectName)
		{
			if (_cache == null)
			{
				return;
			}
			_cache.Remove(stateObjectName);
		}

        /// <summary>
        /// This will brute force remove all items in Cache
        /// For a more focused approach remove each Cache Dependency file
        /// </summary>
        public static void ClearState()
        {
            if (_cache == null)
            {
                return;
            }

            System.Collections.IDictionaryEnumerator enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ClearState(enumerator.Key.ToString());
            }
        }
        
		#endregion

		#endregion
	}

	#endregion
}
