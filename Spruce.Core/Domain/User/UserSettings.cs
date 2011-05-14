using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Serialization;

namespace Spruce.Core
{
	public class UserSettings
	{
		public static string _cacheFolder;

		public string ProjectName { get; set; }
		public string IterationName { get; set; }
		public string IterationPath { get; set; }
		public string AreaName { get; set; }
		public string AreaPath { get; set; }
		/// <summary>
		/// This should map to an action name in BugController
		/// </summary>
		public string BugView { get; set; }
		/// <summary>
		/// This should map to an action name in TaskController
		/// </summary>
		public string TaskView { get; set; }
		public FilterType FilterType { get; set; }

		static UserSettings()
		{
			_cacheFolder = string.Format(@"{0}\App_Data\usersettings\", AppDomain.CurrentDomain.BaseDirectory);

			try
			{
				if (!Directory.Exists(_cacheFolder))
				{
					Directory.CreateDirectory(_cacheFolder);
				}
			}
			catch (IOException)
			{
				// TODO: Warn
			}
		}

		/// <summary>
		/// For now, the user settings database is just an in-memory store
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static UserSettings Load(Guid userId)
		{
			string filename = string.Format(@"{0}\{1}.xml", _cacheFolder, userId);

			if (!File.Exists(filename))
				return new UserSettings();

			try
			{
				using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
					UserSettings settings = (UserSettings) serializer.Deserialize(stream);

					if (settings == null)
						return new UserSettings();
					else
						return settings;
				}
			}
			catch (IOException)
			{
				// TODO: Warn
				return new UserSettings();
			}
			catch (FormatException)
			{
				// TODO: Warn
				return new UserSettings();
			}
			catch (Exception)
			{
				// TODO: Warn
				return new UserSettings();
			}
		}

		public static void Save(Guid userId,UserSettings settings)
		{
			string filename = string.Format(@"{0}\{1}.xml", _cacheFolder, userId);

			try
			{
				using (FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
					serializer.Serialize(stream,settings);
				}
			}
			catch (IOException)
			{
				// TODO: Warn
			}
			catch (FormatException)
			{
				// TODO: Warn
			}
			catch (Exception)
			{
				// TODO: Warn
			}
		}
	}
}