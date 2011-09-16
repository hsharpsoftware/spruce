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
		private static IUserSettingsProvider _persister;

		/// <summary>
		/// The unique id of the object (this property name is used by RavenDb)
		/// </summary>
		public string Id
		{
			get { return UserId.ToString(); }
		}
		
		/// <summary>
		/// This ID comes from TFS based on its authentication.
		/// </summary>
		///
		public Guid UserId { get; protected set; }

		public string Name { get; set; }
		public string ProjectName { get; set; }
		public string IterationName { get; set; }
		public string IterationPath { get; set; }
		public string AreaName { get; set; }
		public string AreaPath { get; set; }
		public string BugView { get; set; }
		public List<ProjectFilterOptions> ProjectFilterOptions { get; protected set; }
		public int PageSize { get; set; }

		static UserSettings()
		{
			// In future versions we can get clever and inject the type we want with Unity, if needed.
			// For now this can use RavenDb by default
			_persister = new RavenDbProvider();
		}

		public UserSettings()
		{
			ProjectFilterOptions = new List<ProjectFilterOptions>();
		}

		public UserSettings(Guid id) : this()
		{
			UserId = id;
		}


		public static UserSettings Load(Guid userId)
		{
			return _persister.Load(userId);
		}

		public static void Save(UserSettings settings)
		{
			_persister.Save(settings);
		}

		public void UpdateBugFilterOptions(string projectName, string title, string assignedTo, string startDate, string endDate, string status)
		{
			ProjectFilterOptions options = GetFilterOptionsForProject(projectName);
			options.BugFilterOptions = FilterOptions.Parse(title, assignedTo, startDate, endDate, status);

			_persister.Save(this);
		}

		public void UpdateBugHeatmapFilterOptions(string projectName, string title, string assignedTo, string startDate, string endDate, string status)
		{
			ProjectFilterOptions options = GetFilterOptionsForProject(projectName);
			options.BugHeatmapFilterOptions = FilterOptions.Parse(title, assignedTo, startDate, endDate, status);

			_persister.Save(this);
		}

		public void UpdateTaskFilterOptions(string projectName, string title, string assignedTo, string startDate, string endDate, string status)
		{
			ProjectFilterOptions options = GetFilterOptionsForProject(projectName);
			options.TaskFilterOptions = FilterOptions.Parse(title, assignedTo, startDate, endDate, status);

			_persister.Save(this);
		}

		public ProjectFilterOptions GetFilterOptionsForProject(string projectName)
		{
			int index = ProjectFilterOptions.IndexOf(projectName);

			if (index == -1)
			{
				ProjectFilterOptions options = new ProjectFilterOptions(projectName);
				ProjectFilterOptions.Add(options);
				return options;
			}
			else
			{
				return ProjectFilterOptions[index];
			}
		}

		public override bool Equals(object obj)
		{
			UserSettings settings = obj as UserSettings;

			if (settings == null)
				return false;

			return settings.UserId == UserId;
		}

		public override int GetHashCode()
		{
			return UserId.GetHashCode();
		}
	}
}