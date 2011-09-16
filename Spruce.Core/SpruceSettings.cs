using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Spruce.Core
{
	public class SpruceSettings
	{
		private static string _uploadDirectory;
		private static string _userSettingsDirectory;

		public static string TfsServer
		{
			get
			{
				return SpruceSection.Current.TfsServer;
			}
		}

		public static string DefaultProjectName
		{
			get
			{
				return SpruceSection.Current.DefaultProjectName;
			}
		}

		/// <summary>
		/// The directory for all attachments to be uploaded prior to being saved on TFS. Includes a trailing slash.
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
		/// The directory for user settings databases or XML files.
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