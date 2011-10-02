using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class TaskManager : WorkItemManager
	{
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
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

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
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public override WorkItemSummary NewItem()
		{
			TaskSummary summary = new TaskSummary();
			WorkItem item = CreateWorkItem(summary.WorkItemType, summary);

			summary.PopulateAllowedValues(item);
			summary.Priority = int.Parse(summary.ValidPriorities[0]);

			summary.EstimatedHours = item.Fields["Remaining Work"].Value.ToString().ToDoubleOrDefault();
			summary.CompletedHours = item.Fields["Completed Work"].Value.ToString().ToDoubleOrDefault();

			return summary;
		}
	}
}
