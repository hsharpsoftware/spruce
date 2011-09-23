using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class QueryManager
	{
		protected QueryBuilder _builder;

		public QueryManager()
		{
			_builder = new QueryBuilder();
		}

		protected void ContrainType(string workItemType)
		{
			_builder.And(string.Format("[Work Item Type]='{0}'",workItemType));
		}

		public T ItemById<T>(int id) where T: WorkItemSummary
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);

			T summary = default(T);
			summary.FromWorkItem(item);
			summary.IsNew = false;

			return summary;
		}

		public void WithTitle(string title)
		{
			_builder.AddParameter("title", title);
			_builder.And("[Title] CONTAINS @title");
		}

		public void SetActive()
		{
			_builder.Or("State='Active'");
		}

		public void SetResolved()
		{
			_builder.Or("State='Resolved'");
		}

		public void SetClosed()
		{
			_builder.Or("State='Closed'");
		}

		public void AndAssignedTo(string name)
		{
			_builder.AddParameter("user", name);
			_builder.And("[Assigned To]=@user");
		}

		public void WithStartingFromDate(DateTime start)
		{
			_builder.AddParameter("datestart", start);
			_builder.And("[System.CreatedDate] >= @datestart");
		}

		public void WithEndingOnDate(DateTime end)
		{
			_builder.AddParameter("dateend", end);
			_builder.And("[System.CreatedDate] < @dateend");
		}

		public IEnumerable<WorkItemSummary> StoredQuery(Guid id)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			Project project = UserContext.Current.WorkItemStore.Projects[UserContext.Current.CurrentProject.Name];
			QueryItem item = project.QueryHierarchy.Find(id);
			
			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(project.StoredQueries[id].QueryText, parameters);
			return collection.ToSummaryList();
		}

		public IEnumerable<WorkItemSummary> AllItems()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} " +
				"ORDER BY Id DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, parameters);
			return collection.ToSummaryList();
		}

		public IEnumerable<IterationSummary> IterationsForProject(string projectName)
		{
			List<IterationSummary> list = new List<IterationSummary>();

			foreach (Node areaNode in UserContext.Current.WorkItemStore.Projects[projectName].IterationRootNodes)
			{
				list.Add(new IterationSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			return list;
		}

		public IEnumerable<AreaSummary> AreasForProject(string projectName)
		{
			List<AreaSummary> list = new List<AreaSummary>();

			foreach (Node areaNode in UserContext.Current.WorkItemStore.Projects[projectName].AreaRootNodes)
			{
				list.Add(new AreaSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			return list;
		}

		public virtual IEnumerable<string> GetAllowedValuesForState(WorkItemType type,string state, string fieldName)
		{
			WorkItem item = new WorkItem(type);
			item.State = state;
			item.Validate();

			return item.Fields[fieldName].AllowedValues.ToList();
		}

		public IEnumerable<WorkItemSummary> Execute()
		{
			string query = _builder.ToString();

			HttpContext.Current.Items["Query"] = MvcHtmlString.Create(query);
			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, _builder.Parameters);
			return collection.ToSummaryList();
		}

		public IEnumerable<WorkItemSummary> ExecuteWiqlQuery(string query, Dictionary<string, object> parameters, bool useDefaultProject)
		{
			if (parameters == null)
				parameters = new Dictionary<string, object>();

			// Add the default project name if one is missing
			if (query.IndexOf("TeamProject") == -1 && useDefaultProject)
			{
				if (!parameters.ContainsKey("Project"))
					parameters.Add("Project", UserContext.Current.CurrentProject.Name);
				else
					parameters["Project"] = UserContext.Current.CurrentProject.Name;

				query += " AND System.TeamProject = @Project";
			}

			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, parameters);
			return collection.ToSummaryList();
		}
	}
}
