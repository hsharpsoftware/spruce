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
	public class UserContext
	{
		private static readonly string CONTEXT_KEY = "SPRUCE_USER_CONTEXT";
		private static UserContext _contextForNoneWeb;
		private string _projectName;
		private List<string> _users;		

		public TfsTeamProjectCollection TfsCollection { get; private set; }
		public WorkItemStore WorkItemStore { get; private set; }
		public VersionControlServer VersionControlServer { get; private set; }
		public RegisteredLinkTypeCollection RegisteredLinkTypes { get; private set; }

		public ProjectDetails CurrentProject { get; private set; }
		public List<string> ProjectNames { get; private set; }
		public string Name { get; private set; }
		public Guid Id { get; private set; }
		public IEnumerable<string> Users
		{
			get
			{
				return _users;
			}
			private set
			{
				_users = new List<string>(value);
			}
		}
		public UserSettings Settings { get; set; }
		
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

		/// <summary>
		/// The current singleton instance of the UserContext.
		/// </summary>
		public static UserContext Current
		{
			get
			{
				if (IsWeb)
				{
					// Use a session instead of HttpContext.Items as Items doesn't survive redirects
					UserContext context = HttpContext.Current.Session[CONTEXT_KEY] as UserContext;
					if (context == null)
					{
						context = new UserContext();
						HttpContext.Current.Session[CONTEXT_KEY] = context;
					}

					// This call is here and not in SpruceApplication as it needs a user login in to 
					// scan the types, as it creates a WorkItem to discover its WorkItemType.
					if (!WorkItemSummaryFactory.HasScanned())
						WorkItemSummaryFactory.Scan();

					return context;
				}
				else
				{
					if (_contextForNoneWeb == null)
						_contextForNoneWeb = new UserContext();

					if (!WorkItemSummaryFactory.HasScanned())
						WorkItemSummaryFactory.Scan();

					return _contextForNoneWeb;
				}
			}
		}

		/// <summary>
		/// Change's the current project for the user.
		/// </summary>
		/// <param name="projectName"></param>
		public void ChangeCurrentProject(string projectName)
		{
			if (string.IsNullOrWhiteSpace(projectName))
				throw new ArgumentNullException("The project name was null or empty");

			if (!WorkItemStore.Projects.Contains(projectName))
				throw new InvalidOperationException(string.Format("The project {0} doesn't exist.", projectName));

			_projectName = projectName;
			CurrentProject = new ProjectDetails(WorkItemStore.Projects[projectName]);

			Settings.ProjectName = projectName;
			Settings.IterationName = CurrentProject.Iterations[0].Name;
			Settings.IterationPath = CurrentProject.Iterations[0].Path;
			Settings.AreaName = CurrentProject.Areas[0].Name;
			Settings.AreaPath = CurrentProject.Areas[0].Path;
		}

		/// <summary>
		/// Populates the <see cref="ProjectNames"/> property with a list of the current TFS projects.
		/// </summary>
		private void PopulateProjectNames()
		{
			ProjectNames = new List<string>();
			foreach (Project project in WorkItemStore.Projects)
			{
				ProjectNames.Add(project.Name);
			}
		}

		/// <summary>
		/// Retrieves all users that TFS uses.
		/// </summary>
		private void PopulateUsers()
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

					_users.Add(name);
				}
			}

			_users.Sort();
		}

		static UserContext()
		{
			IsWeb = true;
		}

		internal UserContext()
		{
			if (string.IsNullOrEmpty(SpruceSettings.TfsServer))
				throw new ArgumentNullException("The TfsServer settings is empty, please set it in the web.config (full URL, including the port)");

			if (string.IsNullOrEmpty(SpruceSettings.DefaultProjectName))
				throw new ArgumentNullException("The DefaultProjectName settings is empty, please set it in the web.config");

			// Connect to TFS
			_users = new List<string>();
			TfsCollection = new TfsTeamProjectCollection(new Uri(SpruceSettings.TfsServer));
			TfsCollection.Authenticate();
			WorkItemStore = new WorkItemStore(TfsCollection);
			VersionControlServer = TfsCollection.GetService<VersionControlServer>();
			RegisteredLinkTypes = WorkItemStore.RegisteredLinkTypes;

			// Get the current username, and load their settings
			Name = TfsCollection.AuthorizedIdentity.DisplayName;
			Id = TfsCollection.AuthorizedIdentity.TeamFoundationId;

			// Load the user settings from the backing store (ravendb)
			Settings = UserSettings.Load(Id);
			Settings.Name = Name;

			// Set the current project, and the view settings
			if (string.IsNullOrEmpty(Settings.ProjectName) || !WorkItemStore.Projects.Contains(Settings.ProjectName))
			{
				ChangeCurrentProject(SpruceSettings.DefaultProjectName);
			}
			else
			{
				_projectName = Settings.ProjectName;
				CurrentProject = new ProjectDetails(WorkItemStore.Projects[_projectName]);
			}

			// Populate the list of project names and users - this is done per user rather
			// than per application, so new project names don't require an app restart.
			PopulateProjectNames();
			PopulateUsers();
		}
	}
}