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

namespace Spruce.Core
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
			item.IterationPath = summary.IterationPath;	
			item.AreaPath = summary.AreaPath;

			if (item.Fields.Contains("Priority"))
				item.Fields["Priority"].Value = (summary.Priority.HasValue) ? summary.Priority.Value : 1;

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
			}
			catch (ValidationException e)
			{
				throw new SaveException(string.Format("Save failed for '{0}' ({1})", item.Title,e.Message), e);
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

			// Default area + iteration
			summary.AreaPath = SpruceContext.Current.FilterSettings.AreaPath;
			summary.IterationPath = SpruceContext.Current.FilterSettings.IterationPath;

			// Populate the valid states
			summary.ValidStates = new List<string>();
			foreach (string state in item.Fields["State"].AllowedValues)
			{
				summary.ValidStates.Add(state);
			}
			summary.State = summary.ValidStates[0];

			// For Bugs: populate the valid priorties
			if (item.Fields.Contains("Priority"))
			{
				summary.ValidPriorities = new List<string>();
				foreach (string state in item.Fields["Priority"].AllowedValues)
				{
					summary.ValidPriorities.Add(state);
				}
				summary.Priority = int.Parse(summary.ValidPriorities[0]);
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
			if (item.Fields.Contains("Priority"))
			{
				foreach (string state in item.Fields["Priority"].AllowedValues)
				{
					summary.ValidPriorities.Add(state);
				}
			}

			return summary;
		}

		public static IList<WorkItemSummary> AllBugs()
		{
			Dictionary<string,string> parameters = new Dictionary<string,string>();
			parameters.Add("project",SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE "+
				"System.TeamProject = @project AND [Work Item Type]='Bug' {0} " +
				"ORDER BY Id DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllItems()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} " +
				"ORDER BY Id DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllClosedBugs()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND State='Closed' {0} " +
				"ORDER BY Id DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllActiveBugs()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Bug' AND State='Active' {0} " +
				"ORDER BY Id DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> ExecuteWiqlQuery(string query, Dictionary<string, object> parameters,bool useDefaultProject)
		{
			if (parameters == null)
				parameters = new Dictionary<string, object>();

			// Add the default project name if one is missing
			if (query.IndexOf("TeamProject") == -1 && useDefaultProject)
			{
				if (!parameters.ContainsKey("Project"))
					parameters.Add("Project", SpruceContext.Current.CurrentProject.Name);
				else
					parameters["Project"] = SpruceContext.Current.CurrentProject.Name;

				query += " AND System.TeamProject = @Project";
			}

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		public static IList<WorkItemSummary> AllTasks()
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("project", SpruceContext.Current.CurrentProject.Name);

			string query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project AND [Work Item Type]='Task' {0} " +
				"ORDER BY Id,State DESC", AddSqlForPaths(parameters));

			WorkItemCollection collection = SpruceContext.Current.WorkItemStore.Query(query, parameters);

			return ToWorkItemSummaryList(collection);
		}

		private static string AddSqlForPaths(Dictionary<string,string> parameters)
		{
			string result = "";

			if (!string.IsNullOrWhiteSpace(SpruceContext.Current.FilterSettings.IterationPath))
			{
				parameters.Add("iteration", SpruceContext.Current.FilterSettings.IterationPath);
				result = " AND [Iteration Path]=@iteration";
			}

			if (!string.IsNullOrWhiteSpace(SpruceContext.Current.FilterSettings.AreaPath))
			{
				parameters.Add("area", SpruceContext.Current.FilterSettings.AreaPath);
				result += " AND [Area Path]=@area";
			}

			return result;
		}

		private static WorkItemSummary ToWorkItemSummary(WorkItem item)
		{
			WorkItemSummary summary = new WorkItemSummary()
			{
				Id = item.Id,
				AssignedTo = item.Fields["Assigned To"].Value.ToString(),
				CreatedDate = item.CreatedDate,
				CreatedBy = item.CreatedBy,
				AreaName = item.AreaPath.Replace(item.Project.Name+"\\",""),
				AreaPath = item.AreaPath,
				Description = item.Description,
				IterationName = item.IterationPath.Replace(item.Project.Name+"\\",""),
				IterationPath = item.IterationPath,
				ResolvedBy = GetFieldValue(item,"Resolved By"),
				State = item.State,
				Title = item.Title,
				IsBug = (item.Type.Name.ToLower() == "bug"),
				ProjectName = item.Project.Name
			};

			if (item.Fields.Contains("Priority"))
				summary.Priority = item.Fields["Priority"].Value.ToIntOrDefault();

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