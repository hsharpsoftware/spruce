using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Server;
using System.Xml;
using System.Web.Mvc;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Spruce.Core
{
	public class SpruceContext
	{
		private static readonly string CONTEXT_KEY = "SPRUCE_CONTEXT";
		private string _projectName;
		private static SpruceContext _contextForNoneWeb;

		public TfsTeamProjectCollection TfsCollection { get; private set; }
		public WorkItemStore WorkItemStore { get; private set; }
		public VersionControlServer VersionControlServer { get; private set; }

		public ProjectDetails CurrentProject { get; private set; }
		public List<string> ProjectNames { get; private set; }
		public string CurrentUser { get; private set; }
		public Guid CurrentUserId { get; private set; }
		public List<string> Users { get; private set; }
		public UserSettings UserSettings { get; set; }
		public List<FilterTypeSummary> FilterTypes { get; set; }
		public static bool IsWeb { get; set; }
		public bool IsMobileBrowser
		{
			get
			{
				if (IsWeb)
				{
					// Android, Blackberry + iPhone only for now
					string userAgent = HttpContext.Current.Request.UserAgent.ToLower();
					return (userAgent.IndexOf("iphone") > -1 || userAgent.IndexOf("android") >-1 || userAgent.IndexOf("blackBerry") > -1);
				}
				else
				{
					return false;
				}
			}
		}

		public static SpruceContext Current
		{
			get
			{
				if (IsWeb)
				{
					// Use a session instead of HttpContext.Items as Items doesn't survive redirects
					SpruceContext context = HttpContext.Current.Session[CONTEXT_KEY] as SpruceContext;
					if (context == null)
					{
						context = new SpruceContext();
						HttpContext.Current.Session[CONTEXT_KEY] = context;
					}

					return context;
				}
				else
				{
					if (_contextForNoneWeb == null)
						_contextForNoneWeb = new SpruceContext();

					return _contextForNoneWeb;
				}
			}
		}

		static SpruceContext()
		{
			IsWeb = true;
		}

		public SpruceContext()
		{
			if (string.IsNullOrEmpty(SpruceSettings.TfsServer))
				throw new ArgumentNullException("The TfsServer settings is empty, please set it in the web.config (full URL, including the port)");

			if (string.IsNullOrEmpty(SpruceSettings.DefaultProjectName))
				throw new ArgumentNullException("The DefaultProjectName settings is empty, please set it in the web.config");

			TfsCollection = new TfsTeamProjectCollection(new Uri(SpruceSettings.TfsServer));
			TfsCollection.Authenticate();
			WorkItemStore = new WorkItemStore(TfsCollection);
			VersionControlServer = TfsCollection.GetService<VersionControlServer>();

			CurrentUser = TfsCollection.AuthorizedIdentity.DisplayName;
			CurrentUserId = TfsCollection.AuthorizedIdentity.TeamFoundationId;
			UserSettings = UserSettings.Load(CurrentUserId);

			if (string.IsNullOrEmpty(UserSettings.ProjectName) || !WorkItemStore.Projects.Contains(UserSettings.ProjectName))
			{
				SetCurrentProject(SpruceSettings.DefaultProjectName);

				UserSettings.BugView = "Index";
				UserSettings.TaskView = "Index";
			}
			else
			{
				_projectName = UserSettings.ProjectName;
				CurrentProject = new ProjectDetails(WorkItemStore.Projects[_projectName]);
			}

			AddProjectNames();
			AddUsers();
			AddFilterNames();
		}

		public void UpdateUserSettings()
		{
			UserSettings.Save(CurrentUserId,UserSettings);
		}

		public void SetCurrentProject(string projectName)
		{
			if (string.IsNullOrWhiteSpace(projectName))
				throw new ArgumentNullException("The project name was null or empty");

			if (!WorkItemStore.Projects.Contains(projectName))
				throw new InvalidOperationException(string.Format("The project {0} doesn't exist.", projectName));

			_projectName = projectName;
			CurrentProject = new ProjectDetails(WorkItemStore.Projects[projectName]);

			UserSettings.ProjectName = projectName;
			UserSettings.IterationName = CurrentProject.Iterations[0].Name;
			UserSettings.IterationPath = CurrentProject.Iterations[0].Path;
			UserSettings.AreaName = CurrentProject.Areas[0].Name;
			UserSettings.AreaPath = CurrentProject.Areas[0].Path;
			UserSettings.FilterType = FilterType.All;
		}

		private void AddProjectNames()
		{
			ProjectNames = new List<string>();
			foreach (Project project in WorkItemStore.Projects)
			{
				ProjectNames.Add(project.Name);
			}
		}

		private void AddFilterNames()
		{
			FilterTypes = new List<FilterTypeSummary>();
			List<string> names = new List<string>(Enum.GetNames(typeof(FilterType)));
			
			foreach (string name in names)
			{
				FilterType filterType;
				Enum.TryParse<FilterType>(name, out filterType);

				FilterTypeSummary summary = new FilterTypeSummary() 
				{ 
					Name = name, 
					Description = filterType.GetDescription() 
				};

				FilterTypes.Add(summary);
			}
		}

		public void AddUsers()
		{
			Users = new List<string>();

			// These must use QueryMembership.Expanded otherwise additional information doesn't get returned
			IGroupSecurityService service = TfsCollection.GetService<IGroupSecurityService>();

			Identity usersInCollection = service.ReadIdentity(SearchFactor.AccountName, "Project Collection Valid Users", QueryMembership.Expanded);
			Identity[] members = service.ReadIdentities(SearchFactor.Sid, usersInCollection.Members, QueryMembership.Expanded);

			for (int i = 0; i < members.Length; i++)
			{
				// Basic filtering
				if (!members[i].SecurityGroup )//&& !usernames.Contains(username))
				{
					string name = members[i].DisplayName;
					if (string.IsNullOrEmpty(name))
						name = members[i].AccountName;

					Users.Add(name);
				}
			}

			Users.Sort();
		}
	}
}