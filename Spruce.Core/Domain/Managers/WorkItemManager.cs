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

		public void AssignedToMe()
		{
			_parameters.Add("user", UserContext.Current.Name);
			_andFilters.Add("[Assigned To]=@user");
		}

		public void Today()
		{
			_parameters.Add("today", DateTime.Today);
			_andFilters.Add("[System.CreatedDate] >= @today");
		}

		public void Yesterday()
		{
			_parameters.Add("yesterdaystart", DateTime.Today.Yesterday());
			_parameters.Add("yesterdayend", DateTime.Today);
			_andFilters.Add("[System.CreatedDate] >= @yesterdaystart AND [System.CreatedDate] <= @yesterdayend");
		}

		public void ThisWeek()
		{
			_parameters.Add("thisweek", DateTime.Today.StartOfWeek());
			_andFilters.Add("[System.CreatedDate] >= @thisweek");
		}

		public void ThisMonth()
		{
			_parameters.Add("thismonth", DateTime.Today.StartOfThisMonth());
			_andFilters.Add("[System.CreatedDate] >= @thismonth");
		}

		public void LastMonth()
		{
			_parameters.Add("lastmonth", DateTime.Today.StartOfLastMonth());
			_parameters.Add("lastmonthend", DateTime.Today.StartOfThisMonth());
			_andFilters.Add("[System.CreatedDate] >= @lastmonth AND [System.CreatedDate] < @lastmonthend");
		}

		#region Statics
		public static IList<WorkItemSummary> ExecuteWiqlQuery(string query, Dictionary<string, object> parameters, bool useDefaultProject)
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

		internal static string AddSqlForPaths(Dictionary<string, object> parameters)
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

		public static WorkItemSummary ItemById(int id)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);
			WorkItemSummary summary = ToWorkItemSummary(item);

			// TODO: useful error messages when there are no states or priorities

			// Populate the valid states, priorties
			summary.ValidStates = new List<string>();
			foreach (string state in item.Fields["State"].AllowedValues)
			{
				summary.ValidStates.Add(state);
			}

			summary.ValidPriorities = new List<string>();
			if (item.Fields.Contains("Priority"))
			{
				foreach (string state in item.Fields["Priority"].AllowedValues)
				{
					summary.ValidPriorities.Add(state);
				}
			}

			summary.ValidSeverities = new List<string>();
			if (item.Fields.Contains("Severity"))
			{
				foreach (string state in item.Fields["Severity"].AllowedValues)
				{
					summary.ValidSeverities.Add(state);
				}
			}

			return summary;
		}

		public static IList<WorkItemSummary> StoredQuery(Guid id)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			Project project = UserContext.Current.WorkItemStore.Projects[UserContext.Current.CurrentProject.Name];
			QueryItem item = project.QueryHierarchy.Find(id);
			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(project.StoredQueries[id].QueryText,parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllItems()
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("project", UserContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} " +
				"ORDER BY Id DESC", WorkItemManager.AddSqlForPaths(parameters));

			WorkItemCollection collection = UserContext.Current.WorkItemStore.Query(query, parameters);

			return WorkItemManager.ToWorkItemSummaryList(collection);
		}

		/// <summary>
		/// Accomodates fields that won't necessarily exist (such as resolved by) until a later stage of the work item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		internal static string GetFieldValue(WorkItem item, string fieldName)
		{
			if (item.Fields.Contains(fieldName))
				return Convert.ToString(item.Fields[fieldName].Value);
			else
				return "";
		}

		internal static WorkItemSummary ToWorkItemSummary(WorkItem item)
		{
			WorkItemSummary summary = new WorkItemSummary()
			{
				Id = item.Id,
				AssignedTo = item.Fields["Assigned To"].Value.ToString(),
				CreatedDate = item.CreatedDate,
				CreatedBy = item.CreatedBy,
				AreaName = item.AreaPath.Replace(item.Project.Name + "\\", ""),
				AreaPath = item.AreaPath,
				Description = item.Description,
				IterationName = item.IterationPath.Replace(item.Project.Name + "\\", ""),
				IterationPath = item.IterationPath,
				ResolvedBy = GetFieldValue(item, "Resolved By"),
				State = item.State,
				Title = item.Title,
				IsBug = (item.Type.Name.ToLower() == "bug"),
				ProjectName = item.Project.Name,
				Fields = item.Fields,
				Attachments = item.Attachments,
				Links = item.Links,
				Revisions = item.Revisions,
			};

			if (item.Fields.Contains("Priority"))
				summary.Priority = item.Fields["Priority"].Value.ToIntOrDefault();

			if (item.Fields.Contains("Severity"))
				summary.Severity = item.Fields["Severity"].Value.ToString();

			// For tasks: estimates
			if (item.Type.Name.ToLower() == "task")
			{
				// Check this field exists. 4.2 Agile seems to have it missing
				if (item.Fields.Contains("Original Estimate"))
					summary.EstimatedHours = item.Fields["Original Estimate"].Value.ToDoubleOrDefault();

				summary.RemainingHours = item.Fields["Remaining Work"].Value.ToDoubleOrDefault();
				summary.CompletedHours = item.Fields["Completed Work"].Value.ToDoubleOrDefault();
			}

			// For CMMI projects
			if (!string.IsNullOrEmpty("Repro Steps") && string.IsNullOrEmpty(item.Description))
				summary.Description = GetFieldValue(item, "Repro Steps");

			return summary;
		}

		internal static IList<WorkItemSummary> ToWorkItemSummaryList(WorkItemCollection collection)
		{
			List<WorkItemSummary> list = new List<WorkItemSummary>();

			foreach (WorkItem item in collection)
			{
				list.Add(ToWorkItemSummary(item));
			}

			return list;
		}

		public static IList<IterationSummary> IterationsForProject(string projectName)
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

		public static IList<AreaSummary> AreasForProject(string projectName)
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

		public static void SaveBug(WorkItemSummary summary)
		{
			Save(summary, UserContext.Current.CurrentProject.WorkItemTypeForBug);
		}

		public static void SaveTask(WorkItemSummary summary)
		{
			Save(summary, UserContext.Current.CurrentProject.WorkItemTypeForTask);
		}

		public static void SaveExisting(WorkItemSummary summary)
		{
			Save(summary, null);
		}

		/// <summary>
		/// Saves an existing or new work item. The summary.Id is updated once saved, if saving a new work item.
		/// </summary>
		/// <param name="summary"></param>
		/// <param name="type"></param>
		public static void Save(WorkItemSummary summary, WorkItemType type)
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

			// For tasks
			if (item.Type.Name.ToLower() == "task")
			{
				// For updates only
				if (item.Fields.Contains("Original Estimate"))
					item.Fields["Original Estimate"].Value = summary.EstimatedHours;

				item.Fields["Remaining Work"].Value = summary.RemainingHours;
				item.Fields["Completed Work"].Value = summary.CompletedHours;
			}

			// For CMMI projects
			if (item.Fields.Contains("Repro Steps"))
				item.Fields["Repro Steps"].Value = summary.Description;

			if (item.Fields.Contains("Symptom"))
				item.Fields["Symptom"].Value = summary.Description;

			if (item.Fields.Contains("Reason") && item.Fields["Reason"].AllowedValues.Count > 0)
				item.Fields["Reason"].Value = item.Fields["Reason"].AllowedValues[0];

			try
			{
				item.Save();
				summary.Id = item.Id;
			}
			catch (ValidationException e)
			{
				throw new SaveException(string.Format("Save failed for '{0}' ({1})", item.Title,e.Message), e);
			}
		}

		public static void SaveAttachments(int id, IEnumerable<Attachment> attachments)
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

		public static void Resolve(int id)
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

		public static void Close(int id)
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

		public static void DeleteAttachment(int id, string url)
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

		public static WorkItemSummary NewTask()
		{
			return NewItem(UserContext.Current.CurrentProject.WorkItemTypeForTask);
		}

		public static WorkItemSummary NewBug()
		{
			return NewItem(UserContext.Current.CurrentProject.WorkItemTypeForBug);
		}

		public static WorkItemSummary NewItem(WorkItemType type)
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
			summary.ValidStates = new List<string>();
			foreach (string state in item.Fields["State"].AllowedValues)
			{
				summary.ValidStates.Add(state);
			}
			summary.State = summary.ValidStates[0];

			// For Bugs: populate the valid priorties and severities
			if (item.Fields.Contains("Priority"))
			{
				summary.ValidPriorities = new List<string>();
				foreach (string state in item.Fields["Priority"].AllowedValues)
				{
					summary.ValidPriorities.Add(state);
				}
				summary.Priority = int.Parse(summary.ValidPriorities[0]);
			}

			if (item.Fields.Contains("Severity"))
			{
				summary.ValidSeverities = new List<string>();
				foreach (string severity in item.Fields["Severity"].AllowedValues)
				{
					summary.ValidSeverities.Add(severity);
				}
				summary.Severity = summary.ValidPriorities[0];
			}

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
		#endregion
	}
}