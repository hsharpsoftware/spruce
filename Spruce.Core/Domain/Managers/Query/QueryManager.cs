using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections;

namespace Spruce.Core
{
	/// <summary>
	/// Provides a set of common querying methods for work items.
	/// </summary>
	/// <typeparam name="T">The <see cref="WorkItemSummary"/> type the querying is for. This constrains the WIQL using the [Work Item Type]= field.</typeparam>
	public class QueryManager<T> where T : WorkItemSummary, new()
	{
		protected QueryBuilder _builder;
		private string _constrainedQuery;

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryManager&lt;T&gt;"/> class.
		/// </summary>
		public QueryManager()
		{
			_builder = new QueryBuilder();
		}

		/// <summary>
		/// Sets the work item type this search is for.
		/// </summary>
		protected void ContrainType(string workItemType)
		{
			if (!string.IsNullOrEmpty(workItemType))
			{
				_builder.And(string.Format("[Work Item Type]='{0}'", workItemType));
			}
		}


		/// <summary>
		/// Retrieves a <see cref="WorkItemSummary"/> or derived class for the given ID.
		/// </summary>
		public T ItemById<T>(int id) where T: WorkItemSummary, new()
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);

			T summary = new T();
			summary.FromWorkItem(item);
			summary.PopulateAllowedValues(item);
			summary.IsNew = false;

			return summary;
		}

		/// <summary>
		/// Retrieves a <see cref="WorkItemSummary"/> or derived class for the given ID and revision number.
		/// </summary>
		public T ItemByIdForRevision<T>(int id, int revisionId) where T : WorkItemSummary, new()
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id,revisionId);

			T summary = new T();
			summary.FromWorkItem(item);
			summary.PopulateAllowedValues(item);
			summary.IsNew = false;

			return summary;
		}

		/// <summary>
		/// Retrieves a <see cref="WorkItemSummary"/> for the given ID.
		/// </summary>
		public WorkItemSummary ItemById(int id)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);

			WorkItemSummary summary = WorkItemSummaryFactory.GetForType(item.Type);
			summary.FromWorkItem(item);
			summary.PopulateAllowedValues(item);
			summary.IsNew = false;

			return summary;
		}

		/// <summary>
		/// Limits the search using the provided title.
		/// </summary>
		public void WithTitle(string title)
		{
			_builder.AddParameter("title", title);
			_builder.And("[Title] CONTAINS @title");
		}

		/// <summary>
		/// Limits the search to work items with the Active state only.
		/// </summary>
		public void WhereActive()
		{
			_builder.Or("State='Active'");
		}

		/// <summary>
		/// Limits the search to work items with the Resolved state only.
		/// </summary>
		public void WhereResolved()
		{
			_builder.Or("State='Resolved'");
		}

		/// <summary>
		/// Limits the search to work items with the Closed state only.
		/// </summary>
		public void WhereClosed()
		{
			_builder.Or("State='Closed'");
		}

		/// <summary>
		/// Limits the search to work items that are assigned to the provided user.
		/// </summary>
		public void AndAssignedTo(string name)
		{
			_builder.AddParameter("user", name);
			_builder.And("[Assigned To]=@user");
		}

		/// <summary>
		/// Limits the search to work items that were created after the date provided.
		/// </summary>
		public void WithStartingFromDate(DateTime start)
		{
			_builder.AddParameter("datestart", start);
			_builder.And("[System.CreatedDate] >= @datestart");
		}

		/// <summary>
		/// Limits the search to work items that were created before the date provided.
		/// </summary>
		public void WithEndingOnDate(DateTime end)
		{
			_builder.AddParameter("dateend", end);
			_builder.And("[System.CreatedDate] < @dateend");
		}

		/// <summary>
		/// Executes a stored query on the TFS server, returning a collection of <see cref="WorkItemSummary"/> objects.
		/// </summary>
		/// <param name="id">The id of the stored query to run.</param>
		public IEnumerable<WorkItemSummary> StoredQuery(Guid id)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			Project project = UserContext.Current.WorkItemStore.Projects[UserContext.Current.CurrentProject.Name];
			QueryItem item = project.QueryHierarchy.Find(id);

			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(project.StoredQueries[id].QueryText, parameters);
			return collection.ToSummaries();
		}

		/// <summary>
		/// Retrieves all work items for the user's currently selected project, area and iteration.
		/// </summary>
		public IEnumerable<WorkItemSummary> AllItems()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} " +
				"ORDER BY Id DESC", _builder.GenerateWiqlForPaths(parameters));

			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, parameters);
			return collection.ToSummaries();
		}

		/// <summary>
		/// Retrieves all iteration names and paths for the provided project.
		/// </summary>
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

		/// <summary>
		/// Retrieves all area names and paths for the provided project.
		/// </summary>
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

		/// <summary>
		/// Returns all allowed values for the provided work item type's field.
		/// </summary>
		/// <param name="type">The work item type create. Use null for an existing work item.</param>
		/// <param name="id">An id of an existing work item. Use 0 for a new work item.</param>
		/// <param name="fieldName">The field name to retrieve the allowed values for.</param>
		public virtual IEnumerable<string> GetAllowedValuesForState(string fieldName, WorkItemType type, int id = 0, string newState = "")
		{
			// This doesn't work as you can't create a new work item with a resolved state
			WorkItem item;
			if (id == 0)
			{
				item = type.NewWorkItem();
				item.Title = "temp";
			}
			else
			{
				item = ItemById(id).ToWorkItem();
				item.State = newState;
			}
			
			ArrayList results = item.Validate();

			if (results.Count == 0)
			{
				return item.Fields[fieldName].AllowedValues.ToList();
			}
			else
			{
				List<string> resultsList = new List<string>();
				foreach (object result in results)
				{
					Field field = result as Field;
					if (field != null)
						resultsList.Add(field.Name + " is invalid");
					else
						resultsList.Add(result.ToString());
				}

				return resultsList;
			}
		}

		/// <summary>
		/// Retrurns all allowed values for the provided field, in the state given.
		/// </summary>
		public virtual IEnumerable<string> GetAllowedValuesForState<T>(string state, string fieldName) where T : WorkItemSummary, new()
		{
			T summary = new T();

			WorkItem item = new WorkItem(summary.WorkItemType);
			item.State = state;
			item.Validate();

			return item.Fields[fieldName].AllowedValues.ToList();
		}

		/// <summary>
		/// Executes the WIQL query that this instance of <see cref="QueryManager"/> represents, and returns the work items for it.
		/// </summary>
		public IEnumerable<WorkItemSummary> Execute()
		{
			// Filter by a type for the query.
			T summary = new T();
			ContrainType(summary.WIQLTypeName);

			string query = _builder.ToString();

			HttpContext.Current.Items["Query"] = MvcHtmlString.Create(query); // should be done on the controllers
			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, _builder.Parameters);
			return collection.ToSummaries();
		}

		/// <summary>
		/// Executes a WIQL query using the provided query, parameters. If useDefaultProject is set, then the query is 
		/// constrained to the user's currently selected project.
		/// </summary>
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
			return collection.ToSummaries();
		}
	}

	/// <summary>
	/// The base, non generic implementation of the QueryManager.
	/// </summary>
	public class QueryManager :  QueryManager<WorkItemSummary>
	{
	}
}
