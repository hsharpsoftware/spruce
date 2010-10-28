using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Server;
using System.Xml;

namespace Spruce.Models
{
	public class SpruceContext
	{
		private static readonly string CONTEXT_KEY = "SPRUCE_CONTEXT";
		private string _projectName;

		public TfsTeamProjectCollection TfsCollection { get; private set; }
		public WorkItemStore WorkItemStore { get; private set; }
		public ProjectDetails CurrentProject { get; private set; }
		public List<string> ProjectNames { get; private set; }
		public string CurrentUser { get; private set; }
		public List<string> Users { get; private set; }
		public FilterSettings FilterSettings { get; set; }

		public static SpruceContext Current
		{
			get
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
		}

		public SpruceContext()
		{
			FilterSettings = new FilterSettings(); // permanently store somewhere

			if (string.IsNullOrEmpty(Settings.TfsServer))
				throw new ArgumentNullException("The TfsServer settings is empty, please set it in the web.config (full URL, including the port)");

			if (string.IsNullOrEmpty(Settings.DefaultProjectName))
				throw new ArgumentNullException("The DefaultProjectName settings is empty, please set it in the web.config");

			TfsCollection = new TfsTeamProjectCollection(new Uri(Settings.TfsServer));
			TfsCollection.Authenticate();
			WorkItemStore = new WorkItemStore(TfsCollection);

			CurrentUser = TfsCollection.AuthorizedIdentity.DisplayName;
			SetProject(Settings.DefaultProjectName);
			GetProjectNames();
			GetUsers();
		}

		public void SetProject(string name)
		{
			if (!WorkItemStore.Projects.Contains(name))
				throw new InvalidOperationException(string.Format("The project {0} doesn't exist.", name));

			_projectName = name;
			CurrentProject = new ProjectDetails(WorkItemStore.Projects[name]);
		}

		private void GetProjectNames()
		{
			ProjectNames = new List<string>();
			foreach (Project project in WorkItemStore.Projects)
			{
				ProjectNames.Add(project.Name);
			}
		}

		public void GetUsers()
		{
			Users = new List<string>();

			// These must use QueryMembership.Expanded otherwise additional information doesn't get returned
			IGroupSecurityService service = TfsCollection.GetService<IGroupSecurityService>();
			Identity usersInCollection = service.ReadIdentity(SearchFactor.AccountName, "Project Collection Valid Users", QueryMembership.Expanded);
			Identity[] members = service.ReadIdentities(SearchFactor.Sid, usersInCollection.Members, QueryMembership.Expanded);
			var users = members.Where(u => u.Type == IdentityType.WindowsUser).ToList();

			foreach (var user in users)
			{
				// TODO: Add smarter filtering
				if (user.Domain.ToLower() != "nt authority")
					Users.Add(user.DisplayName);
			}
		}
	}
}