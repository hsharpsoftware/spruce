using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Spruce.Core
{
	/// <summary>
	/// Holds all settings for the Spruce application, including settings from the configuration file.
	/// </summary>
	public class SpruceSettings
	{
		private static string _uploadDirectory;
		private static string _userSettingsDirectory;

		/// <summary>
		/// The TFS server url, from the configuration file.
		/// </summary>
		public static string TfsServer
		{
			get
			{
				return SpruceSection.Current.TfsServer;
			}
		}

		/// <summary>
		/// The project name to default to. This is read from the configuration file.
		/// </summary>
		public static string DefaultProjectName
		{
			get
			{
				return SpruceSection.Current.DefaultProjectName;
			}
		}

		/// <summary>
		/// The URL path for the Spruce installation, e.g. http://localhost/spruce/. This is read from the configuration file.
		/// </summary>
		public static string SiteUrl
		{
			get
			{
				return SpruceSection.Current.SiteUrl;
			}
		}

		/// <summary>
		/// The directory for all attachments to be uploaded prior to being saved on TFS.
		/// This is the 'App_Data\Attachments' folder, and includes a trailing slash.
		/// </summary>
		public static string UploadDirectory
		{
			get
			{
				if (string.IsNullOrEmpty(_uploadDirectory))
					_uploadDirectory = string.Format(@"{0}\App_Data\Attachments\", AppDomain.CurrentDomain.BaseDirectory);

				return _uploadDirectory;
			}
		}

		/// <summary>
		/// The directory for the user settings RavenDB database/XML files.
		/// This is the 'App_Data\UserSettings' folder, and includes a trailing slash.
		/// </summary>
		public static string UserSettingsDirectory
		{
			get
			{
				if (string.IsNullOrEmpty(_userSettingsDirectory))
					_userSettingsDirectory = string.Format(@"{0}\App_Data\UserSettings\", AppDomain.CurrentDomain.BaseDirectory);

				return _userSettingsDirectory;
			}
		}
	}
}