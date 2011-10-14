using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// Overrides the base <see cref="WorkItemManager"/> class to cater for Bug work items.
	/// </summary>
	public class BugManager : WorkItemManager
	{
		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.Resolve"/> method to modify the behaviour so that a resolved by field is included.
		/// </summary>
		public override void Resolve(int id)
		{
			try
			{
				QueryManager manager = new QueryManager();
				BugSummary summary = manager.ItemById<BugSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Resolved";
				Save(summary);
			}
			catch (Exception ex)
			{
				throw new SaveException(ex, "Unable to resolve Bug work item {0}", id);
			}
		}

		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.Close"/> method to modify the behaviour so that a resolved by field is included.
		/// </summary>
		public override void Close(int id)
		{
			try
			{
				QueryManager manager = new QueryManager();
				BugSummary summary = manager.ItemById<BugSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Closed";
				Save(summary);
			}
			catch (Exception ex)
			{
				throw new SaveException(ex, "Unable to close Bug work item {0}", id);
			}
		}

		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.NewItem"/> method to include the severity and priority fields.
		/// </summary>
		public override WorkItemSummary NewItem()
		{
			BugSummary summary = new BugSummary();
			WorkItem item = base.CreateWorkItem(summary.WorkItemType, summary);

			summary.PopulateAllowedValues(item);
			
			// Check if it Priority exists as Agile 4 templates don't have it
			if (item.Fields.Contains("Priority"))
				summary.Priority = int.Parse(summary.ValidPriorities[0]);

			// Check if it Severity exists as Agile 4 templates don't have it
			if (item.Fields.Contains("Severity"))
				summary.Severity = summary.ValidSeverities[0];

			return summary;
		}
	}
}
