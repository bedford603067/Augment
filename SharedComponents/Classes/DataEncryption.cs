#region Using

using System;
using System.Configuration;

#endregion

namespace FinalBuild
{
	public static class DataEncryption
	{
		#region Enums

		public enum ConfigSection { All, AppSettings, ConnectionStrings }

		#endregion

		#region Public Methods

		public static Configuration ProtectedConfiguration( string connectionProvider, ConfigSection configSection, Configuration configuration )
		{
			switch( configSection )
			{
				case ConfigSection.AppSettings:
					configuration = ProtectAppSettings( connectionProvider, configuration );
					break;
				case ConfigSection.ConnectionStrings:
					configuration = ProtectConnectionStrings( connectionProvider, configuration );
					break;
				default:
					configuration = ProtectAppSettings( connectionProvider, configuration );
					configuration = ProtectConnectionStrings( connectionProvider, configuration );
					break;
			}
			return configuration;
		}

		public static Configuration ProtectedConfiguration( string connectionProvider, Configuration configuration )
		{
			configuration = ProtectAppSettings( connectionProvider, configuration );
			configuration = ProtectConnectionStrings( connectionProvider, configuration );
			return configuration;
		}

		#endregion

		#region Private Methods

		private static Configuration ProtectAppSettings( string connectionProvider, Configuration configuration )
		{
			//Ensure Obfuscated App Settings
			ConfigurationSection section = configuration.AppSettings;
			if( !( section.SectionInformation.IsProtected ) )
			{
				section.SectionInformation.ProtectSection( connectionProvider );
			}
			section.SectionInformation.ForceSave = true;
			return configuration;
		}

		private static Configuration ProtectConnectionStrings( string connectionProvider, Configuration configuration )
		{
			//Ensure Obfuscated Connection Strings
			ConfigurationSection section = configuration.ConnectionStrings;
			if( !( section.SectionInformation.IsProtected ) )
			{
				section.SectionInformation.ProtectSection( connectionProvider );
			}
			section.SectionInformation.ForceSave = true;
			return configuration;
		}

		#endregion
	}
}
