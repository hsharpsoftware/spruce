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

		private List<ProjectFilterOptions> _projectFilterOptions;

		public int PageSize { get; set; }

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

		public UserSettings()
		{
			_projectFilterOptions = new List<ProjectFilterOptions>();
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

		public static void Save(Guid userId, UserSettings settings)
		{
			string filename = string.Format(@"{0}\{1}.xml", _cacheFolder, userId);

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
				Log.Warn(e, "An IO error occurred saving the UserSettings file for user id {0}", userId);
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred saving the UserSettings file for user id {0}", userId);
			}
			catch (Exception e)
			{
				Log.Warn(e, "An unhandled exception error occurred saving the UserSettings file for user id {0}", userId);
			}
		}

		public void UpdateBugFilterOptions(string projectName, string title, string assignedTo, string startDate, string endDate, string status)
		{
			ProjectFilterOptions options = GetFilterOptionsForProject(projectName);
			options.BugFilterOptions = FilterOptions.Parse(title, assignedTo, startDate, endDate, status);

			Save(UserContext.Current.Id, this);
		}

		public void UpdateBugHeatmapFilterOptions(string projectName, string title, string assignedTo, string startDate, string endDate, string status)
		{
			ProjectFilterOptions options = GetFilterOptionsForProject(projectName);
			options.BugHeatmapFilterOptions = FilterOptions.Parse(title, assignedTo, startDate, endDate, status);

			Save(UserContext.Current.Id, this);
		}

		public void UpdateTaskFilterOptions(string projectName, string title, string assignedTo, string startDate, string endDate, string status)
		{
			ProjectFilterOptions options = GetFilterOptionsForProject(projectName);
			options.TaskFilterOptions = FilterOptions.Parse(title, assignedTo, startDate, endDate, status);

			Save(UserContext.Current.Id, this);
		}

		public ProjectFilterOptions GetFilterOptionsForProject(string projectName)
		{
			int index = _projectFilterOptions.IndexOf(projectName);

			if (index == -1)
			{
				ProjectFilterOptions options = new ProjectFilterOptions(projectName);
				_projectFilterOptions.Add(options);
				return options;
			}
			else
			{
				return _projectFilterOptions[index];
			}
		}
	}
}