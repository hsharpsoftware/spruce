using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// This is the Model for a Bug WorkItem in Microsoft Agile 4 and 5 templates. The priority, resolved by and severity fields are the main 
	/// additions from the <see cref="WorkItemSummary"/> it derives from.
	/// </summary>
	public class BugSummary : WorkItemSummary
	{
		/// <summary>
		/// Returns "Bug".
		/// </summary>
		public override string WIQLTypeName
		{
			get
			{
				return "Bug";
			}
		}

		/// <summary>
		/// Returns "Bugs".
		/// </summary>
		public override string Controller
		{
			get
			{
				return "Bugs";
			}
		}

		public string ResolvedBy { get; set; }
		public int? Priority { get; set; }
		public string Severity { get; set; }

		public IList<string> ValidPriorities { get; set; }
		public IList<string> ValidSeverities { get; set; }

		/// <summary>
		/// Fills this instance's fields using the values from the provided <see cref="WorkItem"/>.
		/// This includes the Priority, Reason, Original Estimate, Remaining Work and Completed work fields.
		/// </summary>
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

		/// <summary>
		/// Converts this <see cref="BugSummary"/> instance to a new <see cref="WorkItem"/> using the
		/// <see cref="WorkItemType"/> property to create the work item.
		/// </summary>
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

			if (item.Fields.Contains("Priority"))
				item.Fields["Priority"].Value = (Priority.HasValue) ? Priority.Value : 3;

			if (item.Fields.Contains("Severity") && !string.IsNullOrEmpty(Severity))
				item.Fields["Severity"].Value = Severity;

			return item;
		}

		/// <summary>
		/// Overrides the base implementation to populate the <see cref="ValidPriorities"/> and <see cref="ValidSeverities"/> properties.
		/// </summary>
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
