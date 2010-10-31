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
		public static void SaveBug(WorkItemSummary summary)
		{
			Save(summary, SpruceContext.Current.CurrentProject.WorkItemTypeForBug);
		}

		public static void SaveTask(WorkItemSummary summary)
		{
			Save(summary, SpruceContext.Current.CurrentProject.WorkItemTypeForTask);
		}

		public static void SaveExisting(WorkItemSummary summary)
		{
			Save(summary, null);
		}

		public static void Save(WorkItemSummary summary, WorkItemType type)
		{
			WorkItem item;

			if (summary.IsNew)
			{
				// This knows which project to save in using the WorkItemType.
				item = new WorkItem(type);
			}
			else
			{
				item = SpruceContext.Current.WorkItemStore.GetWorkItem(summary.Id);
			}

			item.Title = summary.Title;
			item.Description = summary.Description; // TODO: change to appropriate Field
			item.Fields["Assigned To"].Value = summary.AssignedTo;
			item.Fields["State"].Value = summary.State;
			item.IterationPath = summary.Iteration;
			item.Fields["Priority"].Value = summary.Priority.Value;
			item.AreaPath = summary.Area;

			// For CMMI projects
			if (item.Fields.Contains("Symptom"))
				item.Fields["Symptom"].Value = summary.Description;

			if (item.Fields.Contains("Reason") && item.Fields["Reason"].AllowedValues.Count > 0)
				item.Fields["Reason"].Value = item.Fields["Reason"].AllowedValues[0];

			try
			{
				item.Save();
			}
			catch (ValidationException e)
			{
				
			}
		}

		public static void Resolve(int id)
		{
			try
			{
				WorkItemSummary summary = ItemById(id);
				summary.ResolvedBy = SpruceContext.Current.CurrentUser;
				summary.State = "Resolved";
				SaveExisting(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public static void Close(int id)
		{
			try
			{
				WorkItemSummary summary = ItemById(id);
				summary.ResolvedBy = SpruceContext.Current.CurrentUser;
				summary.State = "Closed";
				SaveExisting(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public static WorkItemSummary NewTask()
		{
			return NewItem(SpruceContext.Current.CurrentProject.WorkItemTypeForTask);
		}

		public static WorkItemSummary NewBug()
		{
			return NewItem(SpruceContext.Current.CurrentProject.WorkItemTypeForBug);
		}

		public static WorkItemSummary NewItem(WorkItemType type)
		{
			WorkItem item = new WorkItem(type);

			WorkItemSummary summary = new WorkItemSummary();
			summary.CreatedBy = SpruceContext.Current.CurrentUser;
			summary.AssignedTo = SpruceContext.Current.CurrentUser;
			summary.IsNew = true;

			// TODO: useful error messages when there are no states or priorities

			// Populate the valid states
			summary.ValidStates = new List<string>();
			foreach (string state in item.Fields["State"].AllowedValues)
			{
				summary.ValidStates.Add(state);
			}
			summary.State = summary.ValidStates[0];

			// Populate the valid priorties
			summary.ValidPriorities = new List<string>();
			foreach (string state in item.Fields["Priority"].AllowedValues)
			{
				summary.ValidPriorities.Add(state);
			}
			summary.Priority = int.Parse(summary.ValidPriorities[0]);


			return summary;
		}

		public static IList<IterationSummary> IterationsForProject(string projectName)
		{
			List<IterationSummary> list = new List<IterationSummary>();

			foreach (Node areaNode in SpruceContext.Current.WorkItemStore.Projects[projectName].IterationRootNodes)
			{
				list.Add(new IterationSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			return list;
		}

		public static IList<AreaSummary> AreasForProject(string projectName)
		{
			List<AreaSummary> list = new List<AreaSummary>();

			foreach (Node areaNode in SpruceContext.Current.WorkItemStore.Projects[projectName].AreaRootNodes)
			{
				list.Add(new AreaSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			return list;
		}
		
		public static WorkItemSummary ItemById(int id)
		{
			WorkItem item = SpruceContext.Current.WorkItemStore.GetWorkItem(id);
			WorkItemSummary summary = ToWorkItemSummary(item);

			// TODO: useful error messages when there are no states or priorities

			// Populate the valid states, priorties
			summary.ValidStates = new List<string>();
			foreach (string state in item.Fields["State"].AllowedValues)
			{
				summary.ValidStates.Add(state);
			}

			summary.ValidPriorities = new List<string>();
			foreach (string state in item.Fields["Priority"].AllowedValues)
			{
				summary.ValidPriorities.Add(state);
			}

			return summary;
		}

		public static IList<WorkItemSummary> AllBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE "+
				"System.TeamProject = '{0}' AND [Work Item Type]='Bug' {1}" +
				"ORDER BY Id DESC", ProjectNameForSql(), AddSqlForPaths());

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllItems()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = '{0}' {1}" +
				"ORDER BY Id DESC", ProjectNameForSql(), AddSqlForPaths());

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllClosedBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Closed' {1}" +
				"ORDER BY Id DESC", ProjectNameForSql(), AddSqlForPaths());
			
			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllActiveBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Active' {1}" +
				"ORDER BY Id DESC", ProjectNameForSql(), AddSqlForPaths());

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllTasks()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = '{0}' AND [Work Item Type]='Task' AND State='Active' {1}" +
				"ORDER BY Id DESC", ProjectNameForSql(), AddSqlForPaths());

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		private static string AddSqlForPaths()
		{
			if (!string.IsNullOrWhiteSpace(SpruceContext.Current.FilterSettings.IterationPath))
			{
				return string.Format("AND [Iteration Path]='{0}'",
					SpruceContext.Current.FilterSettings.IterationPath.Replace("'", "''"));
			}

			if (!string.IsNullOrWhiteSpace(SpruceContext.Current.FilterSettings.AreaPath))
			{
				return string.Format("AND [Area Path]='{0}'",
					SpruceContext.Current.FilterSettings.AreaPath.Replace("'", "''"));
			}

			return "";
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

			// For CMMI projects
			if (!string.IsNullOrEmpty("Repro Steps") && string.IsNullOrEmpty(item.Description))
				summary.Description = GetFieldValue(item, "Repro Steps");

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

		private static string ProjectNameForSql()
		{
			return SpruceContext.Current.CurrentProject.Name.Replace("'", "''");
		}
	}
}