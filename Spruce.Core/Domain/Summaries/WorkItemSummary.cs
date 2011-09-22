using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	[DebuggerDisplay("{Title}")]
	public class WorkItemSummary<TManager> where TManager : WorkItemManager
	{
		private static TManager _manager;

		public int Id { get; set; }
		public string ProjectName { get; set; }
		public bool IsNew { get; set; }

		public string AreaId { get; set; }
		public string AreaPath { get; set; }
		public string IterationId { get; set; }
		public string IterationPath { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public string AssignedTo { get; set; }
		public string AssignedBy { get; set; }
		
		public string State { get; set; }
		public string Reason { get; set; }

		public FieldCollection Fields { get; internal set; }
		public AttachmentCollection Attachments { get; set; }
		public LinkCollection Links { get; set; }
		public RevisionCollection Revisions { get; set; }

		public static TManager Manager
		{
			get
			{
				if (_manager == null)
					_manager = default(TManager);

				return _manager;
			}
		}

		public virtual T ToWorkItemSummary<T>(WorkItem item) where T: WorkItemSummary<TManager>
		{
			T summary = default(T);
			summary.Id = item.Id;
			summary.AssignedTo = item.Fields["Assigned To"].Value.ToString();
			summary.CreatedDate = item.CreatedDate;
			summary.CreatedBy = item.CreatedBy;
			summary.AreaId = item.AreaPath.Replace(item.Project.Name + "\\", "");
			summary.AreaPath = item.AreaPath;
			summary.Description = item.Description;
			summary.IterationId = item.IterationPath.Replace(item.Project.Name + "\\", "");
			summary.IterationPath = item.IterationPath;
			summary.State = item.State;
			summary.Title = item.Title;
			summary.ProjectName = item.Project.Name;
			summary.Fields = item.Fields;
			summary.Attachments = item.Attachments;
			summary.Links = item.Links;
			summary.Revisions = item.Revisions;

			if (item.Fields.Contains("Reason"))
				summary.Reason = item.Fields["Reason"].Value.ToString();

			return summary;
		}

		/// <summary>
		/// Accomodates fields that won't necessarily exist (such as resolved by) until a later stage of the work item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		protected string GetFieldValue(WorkItem item, string fieldName)
		{
			if (item.Fields.Contains(fieldName))
				return Convert.ToString(item.Fields[fieldName].Value);
			else
				return "";
		}
	}

	public class WorkItemSummary : WorkItemSummary<WorkItemManager>
	{

	}

	public class BugSummary : WorkItemSummary<BugManager>
	{
		public string ResolvedBy { get; set; }

		// For bugs
		public int? Priority { get; set; }
		public string Severity { get; set; }

		public virtual BugSummary ToWorkItemSummary(WorkItem item)
		{
			BugSummary summary = base.ToWorkItemSummary<BugSummary>(item);
			summary.ResolvedBy = GetFieldValue(item, "Resolved By");

			if (item.Fields.Contains("Severity"))
				summary.Severity = item.Fields["Severity"].Value.ToString();

			if (item.Fields.Contains("Priority"))
				summary.Priority = item.Fields["Priority"].Value.ToString().ToIntOrDefault();

			if (item.Fields.Contains("Reason"))
				summary.Reason = item.Fields["Reason"].Value.ToString();

			// For CMMI projects
			if (!string.IsNullOrEmpty("Repro Steps") && string.IsNullOrEmpty(item.Description))
				summary.Description = GetFieldValue(item, "Repro Steps");

			return summary;
		}
	}

	public class TaskSummary : WorkItemSummary<TaskManager>
	{
		public int? Priority { get; set; }
		public string ResolvedBy { get; set; }
		public double EstimatedHours { get; set; }
		public double RemainingHours { get; set; }
		public double CompletedHours { get; set; }

		public virtual TaskSummary ToWorkItemSummary(WorkItem item)
		{
			TaskSummary summary = base.ToWorkItemSummary<TaskSummary>(item);
			summary.ResolvedBy = GetFieldValue(item, "Resolved By");

			if (item.Fields.Contains("Priority"))
				summary.Priority = item.Fields["Priority"].Value.ToString().ToIntOrDefault();

			if (item.Fields.Contains("Reason"))
				summary.Reason = item.Fields["Reason"].Value.ToString();

			// Estimates
			// Check this field exists. 4.2 Agile doesn't have this field
			if (item.Fields.Contains("Original Estimate"))
				summary.EstimatedHours = item.Fields["Original Estimate"].Value.ToDoubleOrDefault();

			summary.RemainingHours = item.Fields["Remaining Work"].Value.ToDoubleOrDefault();
			summary.CompletedHours = item.Fields["Completed Work"].Value.ToDoubleOrDefault();

			// For CMMI projects
			if (!string.IsNullOrEmpty("Repro Steps") && string.IsNullOrEmpty(item.Description))
				summary.Description = GetFieldValue(item, "Repro Steps");

			return summary;
		}
	}
}