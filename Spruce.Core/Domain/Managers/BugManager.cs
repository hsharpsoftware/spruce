using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class BugManager
	{
		public static IList<WorkItemSummary> AllBugs()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllActiveBugs()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND State='Active' {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}	

		public static IList<WorkItemSummary> AllResolvedBugs()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND State='Resolved' {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllClosedBugs()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND State='Closed' {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}	

		public static IList<WorkItemSummary> BugsAssignedToMe()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);
			parameters.Add("user", SpruceContext.Current.CurrentUser);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND [Assigned To]=@user {0} " +
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
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND [System.CreatedDate] >= @today {0} " +
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
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND [System.CreatedDate] >= @start AND [System.CreatedDate] <= @end {0} " +
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
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND [System.CreatedDate] >= @start {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}
	}
}
