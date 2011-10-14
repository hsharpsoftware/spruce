using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Spruce.Core
{
	/// <summary>
	/// An file based user settings provider, where each users's settings are stored in an individual XML file,
	/// using the user's id for the filename.
	/// </summary>
	internal class XmlFileProvider : IUserSettingsProvider
	{
		static XmlFileProvider()
		{
			try
			{
				if (!Directory.Exists(SpruceSettings.UserSettingsDirectory))
				{
					Directory.CreateDirectory(SpruceSettings.UserSettingsDirectory);
				}
			}
			catch (IOException e)
			{
				Log.Error(e, "An IO error occurred creating the usersettings directory ({0})", SpruceSettings.UserSettingsDirectory);
			}
		}

		/// <summary>
		/// For now, the user settings database is just an in-memory store
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public UserSettings Load(Guid userId)
		{
			string filename = string.Format(@"{0}\{1}.xml", SpruceSettings.UserSettingsDirectory, userId);

			if (!File.Exists(filename))
				return new UserSettings();

			try
			{
				using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
					UserSettings settings = (UserSettings)serializer.Deserialize(stream);

					if (settings == null)
						return new UserSettings();
					else
						return settings;
				}
			}
			catch (IOException e)
			{
				Log.Warn(e, "An IO error occurred loading the UserSettings file for user id {0}", userId);
				return new UserSettings();
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred loading the UserSettings file for user id {0}", userId);
				return new UserSettings();
			}
			catch (Exception e)
			{
				Log.Warn(e, "An unhandled exception error occurred loading the UserSettings file for user id {0}", userId);
				return new UserSettings();
			}
		}

		/// <summary>
		/// Persists the provided <see cref="UserSettings"/> object to disk.
		/// </summary>
		/// <param name="settings"></param>
		public void Save(UserSettings settings)
		{
			string filename = string.Format(@"{0}\{1}.xml", SpruceSettings.UserSettingsDirectory, settings.UserId);

			try
			{
				using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
					serializer.Serialize(stream, settings);
				}
			}
			catch (IOException e)
			{
				Log.Warn(e, "An IO error occurred saving the UserSettings file for user {0} id {1}", settings.Name, settings.UserId);
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred saving the UserSettings file for user {0} id {1}", settings.Name, settings.UserId);
			}
			catch (Exception e)
			{
				Log.Warn(e, "An unhandled exception error occurred saving the UserSettings file for user {0} id {1}", settings.Name, settings.UserId);
			}
		}
	}
}
