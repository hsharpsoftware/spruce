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
using Microsoft.TeamFoundation;
using System.Web.Mvc;
using System.Text;

namespace Spruce.Core
{
	public class WorkItemManager
	{
		protected string _query;
		protected Dictionary<string, object> _parameters;
		protected List<string> _orFilters;
		protected List<string> _andFilters;

		public WorkItemManager()
		{
			_parameters = new Dictionary<string, object>();
			_parameters.Add("project", UserContext.Current.CurrentProject.Name);
			_orFilters = new List<string>();
			_andFilters = new List<string>();

			_query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} %FILTERS% " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(_parameters));
		}

		public IList<WorkItemSummary> ExecuteQuery()
		{
			string additionalFilters = "";

			// This is very simplistic right now
			if (_orFilters.Count > 1)
			{
				additionalFilters = "("+ string.Join(" OR ", _orFilters) +")";
			}
			else if (_orFilters.Count == 1)
			{
				additionalFilters = _orFilters[0];
			}

			if (_andFilters.Count > 1)
			{
				if (_orFilters.Count > 0)
					additionalFilters += " AND ("+ string.Join(" AND ", _andFilters) +")";
				else
					additionalFilters += string.Join(" AND ", _andFilters);
			}
			else if (_andFilters.Count == 1)
			{
				if (_orFilters.Count > 0)
					additionalFilters += " AND ("+ _andFilters[0] +")";
				else
					additionalFilters += _andFilters[0];
			}

			if (!string.IsNullOrEmpty(additionalFilters))
			{
				_query = _query.Replace("%FILTERS%", "AND (" + additionalFilters + ")");
			}
			else
			{
				_query = _query.Replace("%FILTERS%", "");
			}

			HttpContext.Current.Items["Query"] = MvcHtmlString.Create(_query);
			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(_query, _parameters);
			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public void Title(string title)
		{
			_parameters.Add("title", title);
			_andFilters.Add("[Title] CONTAINS @title");
		}

		public void Active()
		{
			_orFilters.Add("State='Active'");
		}

		public void Resolved()
		{
			_orFilters.Add("State='Resolved'");
		}

		public void Closed()
		{
			_orFilters.Add("State='Closed'");
		}

		public void AssignedTo(string name)
		{
			_parameters.Add("user", name);
			_andFilters.Add("[Assigned To]=@user");
		}

		public void StartingFromDate(DateTime start)
		{
			_parameters.Add("datestart", start);
			_andFilters.Add("[System.CreatedDate] >= @datestart");
		}

		public void EndingOnDate(DateTime end)
		{
			_parameters.Add("dateend", end);
			_andFilters.Add("[System.CreatedDate] < @dateend");
		}

		public IList<WorkItemSummary> ExecuteWiqlQuery(string query, Dictionary<string, object> parameters, bool useDefaultProject)
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

			return ToWorkItemSummaryList(collection);
		}

		internal string AddSqlForPaths(Dictionary<string, object> parameters)
		{
			string result = "";

			if (!string.IsNullOrWhiteSpace(UserContext.Current.Settings.IterationPath))
			{
				parameters.Add("iteration", UserContext.Current.Settings.IterationPath);
				result = " AND [Iteration Path]=@iteration";
			}

			if (!string.IsNullOrWhiteSpace(UserContext.Current.Settings.AreaPath))
			{
				parameters.Add("area", UserContext.Current.Settings.AreaPath);
				result += " AND [Area Path]=@area";
			}

			return result;
		}

		public WorkItemSummary ItemById(int id)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);
			WorkItemSummary summary = new WorkItemSummary();
			summary.ToWorkItemSummary<WorkItemSummary>(item);

			// TODO: useful error messages when there are no states or priorities
			//PopulateAllowedValues(item, summary);

			return summary;
		}

		public IList<string> GetAllowedBugValuesForState(string state, string fieldName)
		{
			WorkItem item = new WorkItem(UserContext.Current.CurrentProject.WorkItemTypeForBug);
			item.State = state;
			item.Validate();

			return item.Fields[fieldName].AllowedValues.ToList();
		}

		public IList<string> GetAllowedTaskValuesForState(string state, string fieldName)
		{
			WorkItem item = new WorkItem(UserContext.Current.CurrentProject.WorkItemTypeForTask);
			item.State = state;
			item.Validate();

			return item.Fields[fieldName].AllowedValues.ToList();
		}

		public IList<WorkItemSummary> StoredQuery(Guid id)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			Project project = UserContext.Current.WorkItemStore.Projects[UserContext.Current.CurrentProject.Name];
			QueryItem item = project.QueryHierarchy.Find(id);
			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(project.StoredQueries[id].QueryText,parameters);

			return ToWorkItemSummaryList(collection);
		}

		public IList<WorkItemSummary> AllItems()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} " +
				"ORDER BY Id DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		

		internal IList<WorkItemSummary> ToWorkItemSummaryList(WorkItemCollection collection)
		{
			List<WorkItemSummary> list = new List<WorkItemSummary>();

			foreach (WorkItem item in collection)
			{
				WorkItemSummary summary = new WorkItemSummary();
				list.Add(summary.ToWorkItemSummary<WorkItemSummary>(item));
			}

			return list;
		}

		public IList<IterationSummary> IterationsForProject(string projectName)
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

		public IList<AreaSummary> AreasForProject(string projectName)
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

		public void SaveBug(WorkItemSummary summary)
		{
			Save(summary, UserContext.Current.CurrentProject.WorkItemTypeForBug);
		}

		public void SaveTask(WorkItemSummary summary)
		{
			Save(summary, UserContext.Current.CurrentProject.WorkItemTypeForTask);
		}

		public void SaveExisting(WorkItemSummary summary)
		{
			Save(summary, null);
		}

		/// <summary>
		/// Saves an existing or new work item. The summary.Id is updated once saved, if saving a new work item.
		/// </summary>
		/// <param name="summary"></param>
		/// <param name="type"></param>
		public void Save(WorkItemSummary summary, WorkItemType type)
		{
			WorkItem item;

			if (summary.IsNew)
			{
				UserContext.Current.WorkItemStore.RefreshCache();
				// This knows which project to save in using the WorkItemType.
				item = new WorkItem(type);
			}
			else
			{
				item = UserContext.Current.WorkItemStore.GetWorkItem(summary.Id);
			}

			item.Title = summary.Title;
			item.Description = summary.Description; // TODO: change to appropriate Field
			item.Fields["Assigned To"].Value = summary.AssignedTo;
			item.Fields["State"].Value = summary.State;
			item.IterationPath = summary.IterationPath;	
			item.AreaPath = summary.AreaPath;

			if (item.Fields.Contains("Priority"))
				item.Fields["Priority"].Value = (summary.Priority.HasValue) ? summary.Priority.Value : 3;

			if (item.Fields.Contains("Severity") && !string.IsNullOrEmpty(summary.Severity))
				item.Fields["Severity"].Value = summary.Severity;

			if (item.Fields.Contains("Reason"))
			{
				item.Fields["Reason"].Value = summary.Reason;
			}

			// For tasks. No inheritence yet, this method is definitely hacky and would love some refactoring.
			if (item.Type.Name.ToLower() == "task")
			{
				// For updates only, and Agile v5 has this field
				if (item.Fields.Contains("Original Estimate"))
				{
					item.Fields["Original Estimate"].Value = summary.EstimatedHours;
				}
				else
				{
					if (summary.EstimatedHours > 0)
						throw new SaveException(@"The project template appears to be MS Agile 4 which does not support task hour estaimatsion (or tasks have no 
													Original Estimates field). Please Set the hours value to 0 in order to save.");
				}

				// The exception should be thrown before reaching these for agile 4.
				item.Fields["Remaining Work"].Value = summary.RemainingHours;
				item.Fields["Completed Work"].Value = summary.CompletedHours;
			}

			// For CMMI projects
			if (item.Fields.Contains("Repro Steps"))
				item.Fields["Repro Steps"].Value = summary.Description;

			if (item.Fields.Contains("Symptom"))
				item.Fields["Symptom"].Value = summary.Description;

			try
			{
				item.Save();
				summary.Id = item.Id;
			}
			catch (ValidationException e)
			{
				StringBuilder builder = new StringBuilder();
				foreach (Field field in item.Fields)
				{
					if (field.Status != FieldStatus.Valid)
						builder.AppendLine(string.Format("The '{0}' field has the status {1}", field.Name, field.Status));
				}

				throw new SaveException(string.Format("Save failed for '{0}' ({1}).\nFields: {2}", item.Title,e.Message,builder), e);
			}
		}

		public void SaveAttachments(int id, IEnumerable<Attachment> attachments)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);
			foreach (Attachment attachment in attachments)
			{
				item.Attachments.Add(attachment);
			}

			try
			{
				item.Save();
			}
			catch (ValidationException e)
			{
				throw new SaveException(string.Format("Unable to save attachments for '{0}' ({1})", item.Title, e.Message), e);
			}
		}

		public void Resolve(int id)
		{
			try
			{
				WorkItemSummary summary = ItemById(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Resolved";
				SaveExisting(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public void Close(int id)
		{
			try
			{
				WorkItemSummary summary = ItemById(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Closed";
				SaveExisting(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public void DeleteAttachment(int id, string url)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);
			if (item.Attachments.Count > 0)
			{
				int index = -1;

				// Appears that there is no smarter way of doing this with AttachmentCollection
				for (int i = 0; i < item.Attachments.Count; i++)
				{
					if (item.Attachments[i].Uri.ToString() == url)
					{
						index = i;
						break;
					}
				}

				if (index > -1)
				{

					try
					{
						item.Attachments.RemoveAt(index);
						item.Save();
					}
					catch (ValidationException e)
					{
						throw new SaveException(string.Format("Removing attachment {0} failed for id '{1}' ({2})", url, id, e.Message), e);
					}
				}
				else
				{
					// TODO: warn
				}
			}
		}

		public WorkItemSummary NewTask()
		{
			return NewItem(UserContext.Current.CurrentProject.WorkItemTypeForTask);
		}

		public WorkItemSummary NewBug()
		{
			return NewItem(UserContext.Current.CurrentProject.WorkItemTypeForBug);
		}

		public WorkItemSummary NewItem(WorkItemType type)
		{
			WorkItem item = new WorkItem(type);

			WorkItemSummary summary = new WorkItemSummary();
			summary.CreatedBy = UserContext.Current.Name;
			summary.AssignedTo = UserContext.Current.Name;
			summary.Fields = item.Fields;
			summary.IsNew = true;

			// Default area + iteration
			summary.AreaPath = UserContext.Current.Settings.AreaPath;
			summary.IterationPath = UserContext.Current.Settings.IterationPath;

			// Populate the valid states
			PopulateAllowedValues(item, summary);
			summary.State = summary.ValidStates[0];
			summary.Priority = int.Parse(summary.ValidPriorities[0]);
			summary.Severity = summary.ValidPriorities[0];
			summary.Reason = summary.ValidReasons[0];

			// For tasks: estimates
			if (item.Type.Name.ToLower() == "task")
			{
				// For updates only
				if (item.Fields.Contains("Original Estimate"))
					summary.EstimatedHours = item.Fields["Original Estimate"].Value.ToDoubleOrDefault();

				summary.EstimatedHours = item.Fields["Remaining Work"].Value.ToDoubleOrDefault();
				summary.CompletedHours = item.Fields["Completed Work"].Value.ToDoubleOrDefault();
			}

			return summary;
		}
	}
}