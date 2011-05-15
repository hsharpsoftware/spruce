using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	/// <summary>
	/// Needs a DRY'ing
	/// </summary>
	public class TaskManager
	{
		public static IList<WorkItemSummary> AllTasks()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' {0} " +
				"ORDER BY Id,State DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllActiveTasks()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' AND State='Active' {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllClosedTasks()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' AND State='Closed' {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> TasksAssignedToMe()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);
			parameters.Add("user", SpruceContext.Current.CurrentUser);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' AND [Assigned To]=@user {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> Today()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);
			parameters.Add("today", DateTime.Today);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' AND [System.CreatedDate] >= @today {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> Yesterday()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);
			parameters.Add("start", DateTime.Today.Yesterday());
			parameters.Add("end", DateTime.Today);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' AND [System.CreatedDate] >= @start AND [System.CreatedDate] <= @end {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> ThisWeek()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);
			parameters.Add("start", DateTime.Today.StartOfWeek());

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' AND [System.CreatedDate] >= @start {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}
	}
}
