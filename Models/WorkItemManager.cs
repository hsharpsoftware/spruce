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
			WorkItem item;

			if (summary.IsNew)
			{
				// This knows which project to save in using the WorkItemType.
				WorkItemType type = SpruceContext.Current.CurrentProject.WorkItemTypeForBug;
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
			
			item.Save();
		}
		
		public static WorkItemSummary ItemById(int id)
		{
			WorkItem item = SpruceContext.Current.WorkItemStore.GetWorkItem(id);
			WorkItemSummary summary = ToWorkItemSummary(item);

			summary.ValidStates = new List<string>();
			foreach (string state in item.Fields["State"].AllowedValues)
			{
				summary.ValidStates.Add(state);
			}

			return summary;
		}

		public static IList<WorkItemSummary> AllBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' ORDER BY Id DESC", ProjectNameForSql());

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllItems()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' ORDER BY Id DESC", ProjectNameForSql());
			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllClosedBugs()
		{	
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Closed' ORDER BY Id DESC", ProjectNameForSql());
			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllActiveBugs()
		{
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Active' ORDER BY Id DESC", ProjectNameForSql());
			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllTasks()
		{		
			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Task' AND State='Active' ORDER BY Id", ProjectNameForSql());
			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query);

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

		private static string ProjectNameForSql()
		{
			return SpruceContext.Current.CurrentProject.Name.Replace("'", "''");
		}
	}
}