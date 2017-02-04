using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spruce.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// Overrides the base <see cref="WorkItemManager"/> class to cater for Task work items.
	/// </summary>
	public class TaskManager : WorkItemManager
	{
		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.Resolve"/> method to modify the behaviour so that a resolved by field is included.
		/// </summary>
		public override void Resolve(int id)
		{
			QueryManager manager = new QueryManager();
			try
			{
				TaskSummary summary = manager.ItemById<TaskSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Resolved";
				Save(summary);
			}
			catch (Exception ex)
			{
				throw new SaveException(ex, "Unable to resolve Task work item {0}", id);
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
				TaskSummary summary = manager.ItemById<TaskSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Closed";
				Save(summary);
			}
			catch (Exception ex)
			{
				throw new SaveException(ex, "Unable to close Task work item {0}", id);
			}
		}

		/// <summary>
		/// Overrides the base <see cref="WorkItemManager.NewItem"/> method to include remaining work,completed work hours and priority fields.
		/// </summary>
		public override WorkItemSummary NewItem()
		{
			TaskSummary summary = new TaskSummary();
			WorkItem item = CreateWorkItem(summary.WorkItemType, summary);

			summary.PopulateAllowedValues(item);

			// Check if it Priority exists as Agile 4 templates don't have it
			if (item.Fields.Contains("Priority"))
				summary.Priority = int.Parse(summary.ValidPriorities[0]);

			if (item.Fields.Contains("Remaining Work") && item.Fields["Remaining Work"].Value != null)
				summary.EstimatedHours = item.Fields["Remaining Work"].Value.ToString().ToDoubleOrDefault();

			if (item.Fields.Contains("Completed Work") && item.Fields["Completed Work"].Value != null)
				summary.CompletedHours = item.Fields["Completed Work"].Value.ToString().ToDoubleOrDefault();

			// No "Remaining Work" field exists when the state is new.

			return summary;
		}
	}
}
