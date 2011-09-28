using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class BugSummary : WorkItemSummary
	{
		public override string WIQLTypeName
		{
			get
			{
				return "Bug";
			}
		}

		public override string Controller
		{
			get
			{
				return "Bugs";
			}
		}

		public string ResolvedBy { get; set; }

		// For bugs
		public int? Priority { get; set; }
		public string Severity { get; set; }

		public IList<string> ValidPriorities { get; set; }
		public IList<string> ValidSeverities { get; set; }

		public override void FromWorkItem(WorkItem item)
		{
			base.FromWorkItem(item);
			ResolvedBy = GetFieldValue(item, "Resolved By");

			if (item.Fields.Contains("Severity"))
				Severity = item.Fields["Severity"].Value.ToString();

			if (item.Fields.Contains("Priority"))
				Priority = item.Fields["Priority"].Value.ToString().ToIntOrDefault();

			if (item.Fields.Contains("Reason"))
				Reason = item.Fields["Reason"].Value.ToString();

			// For CMMI projects
			if (!string.IsNullOrEmpty("Repro Steps") && string.IsNullOrEmpty(item.Description))
				Description = GetFieldValue(item, "Repro Steps");
		}

		public override WorkItem ToWorkItem()
		{
			WorkItem item;

			if (IsNew)
			{
				UserContext.Current.WorkItemStore.RefreshCache();
				// This knows which project to save in using the WorkItemType.
				item = new WorkItem(WorkItemType);
			}
			else
			{
				item = UserContext.Current.WorkItemStore.GetWorkItem(Id);
			}

			FillCoreFieldsFromSummary(item);

			if (item.Fields.Contains("Priority"))
				item.Fields["Priority"].Value = (Priority.HasValue) ? Priority.Value : 3;

			if (item.Fields.Contains("Severity") && !string.IsNullOrEmpty(Severity))
				item.Fields["Severity"].Value = Severity;

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
