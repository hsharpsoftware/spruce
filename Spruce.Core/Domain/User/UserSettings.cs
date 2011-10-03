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
		private SerializableDictionary<string, string> _defaultActions;

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
			_defaultActions = new SerializableDictionary<string, string>();
		}

		public UserSettings(Guid id) : this()
		{
			UserId = id;
		}


		public static UserSettings Load(Guid userId)
		{
			return _persister.Load(userId);
		}

		public void Save()
		{
			_persister.Save(this);
		}

		/// <summary>
		/// This is used to store the default action, if there are different types of views for work items lists.
		/// For example the MSAgile bug list has both "default" and "heatmap". Settings the key "bugs" to "heatmap" 
		/// will ensure that the heatmap view is shown when the the Index controller is called.
		/// </summary>
		public void SetDefaultAction(string workItemType, string actionName)
		{
			if (!_defaultActions.ContainsKey(workItemType))
				_defaultActions.Add(workItemType, actionName);

			_defaultActions[workItemType] = actionName;
		}

		public string GetDefaultAction(string workItemType)
		{
			if (!_defaultActions.ContainsKey(workItemType))
				_defaultActions.Add(workItemType, "Index");

			return _defaultActions[workItemType];
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