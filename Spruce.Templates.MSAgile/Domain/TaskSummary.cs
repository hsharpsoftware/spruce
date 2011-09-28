using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class TaskSummary : WorkItemSummary
	{
		public override string WIQLTypeName
		{
			get
			{
				return "Task";
			}
		}

		public override string Controller
		{
			get
			{
				return "Tasks";
			}
		}

		public int? Priority { get; set; }
		public string ResolvedBy { get; set; }
		public double EstimatedHours { get; set; }
		public double RemainingHours { get; set; }
		public double CompletedHours { get; set; }

		public IList<string> ValidPriorities { get; set; }
		public IList<string> ValidSeverities { get; set; }

		public override void FromWorkItem(WorkItem item)
		{
			base.FromWorkItem(item);

			ResolvedBy = GetFieldValue(item, "Resolved By");

			if (item.Fields.Contains("Priority"))
				Priority = item.Fields["Priority"].Value.ToString().ToIntOrDefault();

			if (item.Fields.Contains("Reason"))
				Reason = item.Fields["Reason"].Value.ToString();

			// Estimates
			// Check this field exists. 4.2 Agile doesn't have this field
			if (item.Fields.Contains("Original Estimate"))
				EstimatedHours = item.Fields["Original Estimate"].Value.ToString().ToDoubleOrDefault();

			RemainingHours = item.Fields["Remaining Work"].Value.ToString().ToDoubleOrDefault();
			CompletedHours = item.Fields["Completed Work"].Value.ToString().ToDoubleOrDefault();

			// For updates only
			if (item.Fields.Contains("Original Estimate"))
				EstimatedHours = item.Fields["Original Estimate"].Value.ToString().ToDoubleOrDefault();
		}

		public override WorkItem ToWorkItem()
		{
			WorkItem item;

			if (IsNew)
			{
				UserContext.Current.WorkItemStore.RefreshCache();
				item = new WorkItem(WorkItemType);
			}
			else
			{
				item = UserContext.Current.WorkItemStore.GetWorkItem(Id);
			}

			FillCoreFieldsFromSummary(item);

			// For updates only, and Agile v5 has this field
			if (item.Fields.Contains("Original Estimate"))
			{
				item.Fields["Original Estimate"].Value = EstimatedHours;
			}
			else
			{
				if (EstimatedHours > 0)
					throw new SaveException(@"The project template appears to be MS Agile 4 which does not support task hour estimation (or tasks have no 
													Original Estimates field). Please Set the hours value to 0 in order to save.");
			}

			if (item.Fields.Contains("Priority"))
				item.Fields["Priority"].Value = (Priority.HasValue) ? Priority.Value : 3;

			// The exception should be thrown before reaching these for agile 4.
			if (item.Fields.Contains("Remaining Work"))
				item.Fields["Remaining Work"].Value = RemainingHours;

			if (item.Fields.Contains("Completed Work"))
				item.Fields["Completed Work"].Value = CompletedHours;

			return item;
		}

		public override void PopulateAllowedValues(WorkItem item)
		{
			ValidPriorities = new List<string>();
			ValidSeverities = new List<string>();

			base.PopulateAllowedValues(item);

			// All valid priorities
			ValidPriorities = new List<string>();
			if (item.Fields.Contains("Priority"))
			{
				ValidPriorities = item.Fields["Priority"].AllowedValues.ToList();
			}

			// All valid severities
			ValidSeverities = new List<string>();
			if (item.Fields.Contains("Severity"))
			{
				ValidSeverities = item.Fields["Severity"].AllowedValues.ToList();
			}
		}
	}
}
