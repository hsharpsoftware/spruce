using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class TaskManager : WorkItemManager<TaskSummary>
	{
		public override void Resolve(int id)
		{
			TaskQueryManager manager = new TaskQueryManager();
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
			TaskQueryManager manager = new TaskQueryManager();
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

		public override TaskSummary NewItem()
		{
			TaskSummary summary = new TaskSummary();
			WorkItem item = CreateWorkItem(UserContext.Current.CurrentProject.WorkItemTypeForTask, summary);

			TaskItemTemplate template = new TaskItemTemplate();
			summary.Priority = int.Parse(template.ValidPriorities[0]);

			summary.EstimatedHours = item.Fields["Remaining Work"].Value.ToDoubleOrDefault();
			summary.CompletedHours = item.Fields["Completed Work"].Value.ToDoubleOrDefault();

			return summary;
		}
	}
}
