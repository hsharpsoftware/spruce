using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;
using System.Net;
using System.Security.Principal;
using Microsoft.TeamFoundation.Server;
using System.Xml;

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

		public static List<string> GetTriagesItems()
		{
			return GetAllowedValues("Triage");
		}

		public static List<string> GetPriority()
		{
			return GetAllowedValues("Priority");
		}

		public static void New()
		{
			WorkItemType type = GetWorkItemTypes().FirstOrDefault(t => t.Name == "Bug");

			// This knows where to save using the WorkItemType.
			WorkItem item = new WorkItem(type);
			item.Title = "API test";
			item.Description = "Description";
			item.Fields["Assigned To"].Value = "chris";
			item.Save();
		}

		/// <summary>
		/// Reasons, triage are unsupported
		/// Project holds field definitions.
		/// </summary>
		/// <returns></returns>
		public static List<string> GetReasons()
		{
			return GetAllowedValues("Reason");
		}

		public static List<WorkItemType> GetWorkItemTypes()
		{
			List<WorkItemType> list = new List<WorkItemType>();

			foreach (WorkItemType workItemType in _store.Projects[Settings.TfsProject].WorkItemTypes)
			{
				list.Add(workItemType);
			}

			return list;
		}

		public static List<string> GetWorkItemTypesAsStrings()
		{
			List<string> list = new List<string>();

			foreach (WorkItemType workItemType in _store.Projects[Settings.TfsProject].WorkItemTypes)
			{
				list.Add(workItemType.Name);
			}

			return list;
		}

		public static List<string> GetReasonsForType()
		{
			List<string> list = new List<string>();

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' ORDER BY Id DESC", SafeProjectName());
			WorkItemCollection collection = _store.Query(query);

			return list;
		}

		/// <summary>
		/// If this returns an empty list, and Reason isn't empty then the Reason field can't change.
		/// TODO: cache a WorkItem so the field lookup is faster.
		/// </summary>
		/// <returns></returns>
		public static List<string> GetReasonsForType2()
		{
			List<string> list = new List<string>();

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' ORDER BY Id DESC", SafeProjectName());
			WorkItemCollection collection = _store.Query(query);

			foreach (string value in collection[0].Fields["Reason"].AllowedValues)
			{
				list.Add(value);
			}

			return list;
		}

		public static List<string> GetSeverities()
		{
			return GetAllowedValues("Severity");
		}

		public static List<string> GetAllowedValues(string fieldName)
		{
			List<string> list = new List<string>();

			foreach (FieldDefinition definition in _store.FieldDefinitions)
			{
				if (definition.Name == fieldName)
				{
					foreach (string value in definition.AllowedValues)
					{
						list.Add(value);
					}
				}
			}

			return list;
		}

		public static IList<AreaSummary> Areas()
		{
			List<AreaSummary> list = new List<AreaSummary>();

			foreach (Node areaNode in _store.Projects[Settings.TfsProject].AreaRootNodes)
			{
				list.Add(new AreaSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			return list;
		}

		public static IList<IterationSummary> Iterations()
		{
			List<IterationSummary> list = new List<IterationSummary>();

			foreach (Node areaNode in _store.Projects[Settings.TfsProject].IterationRootNodes)
			{
				list.Add(new IterationSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			return list;
		}

		public static IList<AreaSummary> AreasFor2008()
		{
			List<AreaSummary> list = new List<AreaSummary>();

			ICommonStructureService service = _collection.GetService<ICommonStructureService>();
			ProjectInfo projectInfo = service.GetProjectFromName(Settings.TfsProject);
			NodeInfo[] nodes = service.ListStructures(projectInfo.Uri);

			// Areas XML (0):
			// <Node NodeID="xx" Name="Area" Path="\SpruceTest\Area" ProjectID="x" StructureType="ProjectModelHierarchy">
			//    <Children>
			//        <Node NodeID="x" Name="Area 0" ParentID="x" Path="\SpruceTest\Area\Area 0" ProjectID="x" StructureType="ProjectModelHierarchy" />
			//    </Children>
			// </Node>

			XmlElement element = service.GetNodesXml(new string[] { nodes[0].Uri }, true);
			if (element.FirstChild != null && element.FirstChild.ChildNodes.Count > 0)
			{
				int myNodeCount = element.FirstChild.ChildNodes[0].ChildNodes.Count;
				for (int i = 0; i < myNodeCount; i++)
				{
					XmlNode node = element.FirstChild.ChildNodes[0].ChildNodes[i];
					list.Add(new AreaSummary()
					{
						Name = node.Attributes["Name"].Value,
						Path = node.Attributes["Path"].Value
					});
				}
			}

			return list;
		}

		public static IList<IterationSummary> IterationsFor2008()
		{
			List<IterationSummary> list = new List<IterationSummary>();

			ICommonStructureService service = _collection.GetService<ICommonStructureService>();
			ProjectInfo projectInfo = service.GetProjectFromName(Settings.TfsProject);
			NodeInfo[] nodes = service.ListStructures(projectInfo.Uri);

			// Iterations XML (1):
			// <Node NodeID="x" Name="Iteration" Path="\SpruceTest\Iteration" ProjectID="x" StructureType="ProjectLifecycle">
			//    <Children>
			//        <Node NodeID="x" Name="Iteration 1" ParentID="x" Path="\SpruceTest\Iteration\Iteration 1" ProjectID="x" StructureType="ProjectLifecycle" />
			//        <Node NodeID="x" Name="Iteration 2" ParentID="x" Path="\SpruceTest\Iteration\Iteration 2" ProjectID="x" StructureType="ProjectLifecycle" />
			//        <Node NodeID="x" Name="Iteration 3" ParentID="x" Path="\SpruceTest\Iteration\Iteration 3" ProjectID="x" StructureType="ProjectLifecycle" />
			//    </Children>
			// </Node>

			XmlElement element = service.GetNodesXml(new string[] { nodes[1].Uri }, true);
			if (element.FirstChild != null && element.FirstChild.ChildNodes.Count > 0)
			{
				int myNodeCount = element.FirstChild.ChildNodes[0].ChildNodes.Count;
				for (int i = 0; i < myNodeCount; i++)
				{
					XmlNode node = element.FirstChild.ChildNodes[0].ChildNodes[i];
					list.Add(new IterationSummary() 
					{ 
						Name = node.Attributes["Name"].Value, 
						Path = node.Attributes["Path"].Value 
					});
				}
			}

			return list;
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
			// These must use QueryMembership.Expanded otherwise additional information doesn't get returned
			IGroupSecurityService service = _collection.GetService<IGroupSecurityService>();
			Identity usersInCollection = service.ReadIdentity(SearchFactor.AccountName, "Project Collection Valid Users", QueryMembership.Expanded);
			Identity[] members = service.ReadIdentities(SearchFactor.Sid, usersInCollection.Members, QueryMembership.Expanded);

			List<string> list = new List<string>();
			var users = members.Where(u => u.Type == IdentityType.WindowsUser).ToList();

			foreach (var user in users)
			{
				list.Add(user.DisplayName);
			}

			return list;
		}
		
		public static WorkItemSummary ItemById(int id)
		{
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
				Title = item.Title
			};

			if (item.Fields.Contains("Priority"))
				summary.Priority = int.Parse(item.Fields["Priority"].Value.ToString());

			// change to dynamic field for other install types: Model.Fields["Repro Steps"].Value
			if (!string.IsNullOrEmpty(Settings.DescriptionField) && string.IsNullOrEmpty(item.Description))
				summary.Description = GetFieldValue(item,Settings.DescriptionField);

			return summary;
		}

		/// <summary>
		/// Accomodates fields that won't necessarily exist (such as resolved by) until a later stage of the work item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
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