using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;
using System.Net;
using System.Security.Principal;
using Microsoft.TeamFoundation.Server;

namespace Spruce.Models
{
	public class WorkItemManager
	{
		private static TfsTeamProjectCollection _collection;
		private static WorkItemStore _store;

		public static void Configure()
		{
			_collection = new TfsTeamProjectCollection(new Uri(Settings.TfsServer));
			_collection.Authenticate();

			_store = new WorkItemStore(_collection);
		}

		public static IList<string> Projects()
		{
			List<string> list = new List<string>();
			foreach (Project project in _store.Projects)
			{
				list.Add(project.Name);
			}

			return list;
		}

		public static string LoggedInUser()
		{
			return _collection.AuthorizedIdentity.DisplayName;
		}

		public static List<string> Users()
		{
			IGroupSecurityService service = _collection.GetService<IGroupSecurityService>();
			Identity usersInCollection = service.ReadIdentity(SearchFactor.AccountName, "Project Collection Valid Users", QueryMembership.Expanded);
			Identity[] members = service.ReadIdentities(SearchFactor.Sid, usersInCollection.Members, QueryMembership.Expanded);

			List<string> list = new List<string>();
			var users = members.Where(u => u.Type == IdentityType.WindowsUser).ToList();

			foreach (var user in users)
			{
				list.Add(user.AccountName);
			}

			return list;
		}
		
		public static WorkItemSummary ItemById(int id)
		{
			Users();
			WorkItem item = _store.GetWorkItem(id);

			return ToWorkItemSummary(item);
		}

		public static IList<WorkItemSummary> AllBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' ORDER BY Id DESC", SafeProjectName());

			WorkItemCollection collection = _store.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllItems()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' ORDER BY Id DESC", SafeProjectName());
			WorkItemCollection collection = _store.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllClosedBugs()
		{	
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Closed' ORDER BY Id DESC", SafeProjectName());
			WorkItemCollection collection = _store.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllActiveBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Active' ORDER BY Id DESC", SafeProjectName());
			WorkItemCollection collection = _store.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllTasks()
		{		
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Task' AND State='Active' ORDER BY Id", SafeProjectName());
			WorkItemCollection collection = _store.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		private static WorkItemSummary ToWorkItemSummary(WorkItem item)
		{
			foreach (Field field in item.Fields)
			{
				System.Diagnostics.Debug.WriteLine(field.Name);
			}

			WorkItemSummary summary = new WorkItemSummary()
			{
				Id = item.Id,
				AssignedTo = item.Fields["Assigned To"].Value.ToString(),
				CreatedDate = item.CreatedDate,
				CreatedBy = item.CreatedBy,
				Area = item.AreaPath,
				Description = item.Description,
				Iteration = item.IterationPath,
				ResolvedBy = GetFieldValue(item,"Resolved By"),
				State = item.State,
				Title = item.Title,
				Priority = Convert.ToInt32(item.Fields["Priority"].Value)
			};

			// change to dynamic field for other install types: Model.Fields["Repro Steps"].Value
			if (!string.IsNullOrEmpty(Settings.DescriptionField) && string.IsNullOrEmpty(item.Description))
				summary.Description = GetFieldValue(item,Settings.DescriptionField);

			return summary;
		}

		private static string GetFieldValue(WorkItem item, string fieldName)
		{
			if (item.Fields.Contains(fieldName))
				return Convert.ToString(item.Fields[fieldName].Value);
			else
				return "";
		}

		private static IList<WorkItemSummary> ToWorkItemSummaryList(WorkItemCollection collection)
		{
			List<WorkItemSummary> list = new List<WorkItemSummary>();

			foreach (WorkItem item in collection)
			{
				list.Add(ToWorkItemSummary(item));
			}

			return list;
		}

		private static string SafeProjectName()
		{
			return Settings.TfsProject.Replace("'", "''");
		}
	}
}