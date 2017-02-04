using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spruce.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// Overrides the base <see cref="WorkItemManager"/> class to cater for Issue work items.
	/// </summary>
	public class IssueManager : WorkItemManager
	{
		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.Resolve"/> method to modify the behaviour so that a resolved by field is included.
		/// </summary>
		public override void Resolve(int id)
		{
			QueryManager manager = new QueryManager();
			try
			{
				IssueSummary summary = manager.ItemById<IssueSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Resolved";
				Save(summary);
			}
			catch (Exception ex)
			{
				throw new SaveException(ex, "Unable to resolve Issue work item {0}", id);
			}
		}

		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.Close"/> method to modify the behaviour so that a resolved by field is included.
		/// </summary>
		public override void Close(int id)
		{
			QueryManager manager = new QueryManager();
			try
			{
				IssueSummary summary = manager.ItemById<IssueSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Closed";
				Save(summary);
			}
			catch (Exception ex)
			{
				throw new SaveException(ex, "Unable to close Issue work item {0}", id);
			}
		}

		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.NewItem"/> method to include remaining work,completed work hours and priority fields.
		/// </summary>
		public override WorkItemSummary NewItem()
		{
			IssueSummary summary = new IssueSummary();
			WorkItem item = CreateWorkItem(summary.WorkItemType, summary);

			summary.PopulateAllowedValues(item);

			// Not sure if Issues have priority in Agile 4, so add a check (needs confirming)
			if (item.Fields.Contains("Priority"))
				summary.Priority = int.Parse(summary.ValidPriorities[0]);

			return summary;
		}
	}
}
