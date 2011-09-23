using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class BugManager : WorkItemManager<BugSummary>
	{
		public override void Resolve(int id)
		{
			try
			{
				BugQueryManager manager = new BugQueryManager();
				BugSummary summary = manager.ItemById<BugSummary>(id);
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
			try
			{
				BugQueryManager manager = new BugQueryManager();
				BugSummary summary = manager.ItemById<BugSummary>(id);
				summary.ResolvedBy = UserContext.Current.Name;
				summary.State = "Closed";
				Save(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public override BugSummary NewItem()
		{
			BugSummary summary = new BugSummary();
			WorkItem item = base.CreateWorkItem(UserContext.Current.CurrentProject.WorkItemTypeForBug,summary);

			BugItemTemplate template = new BugItemTemplate();
			summary.Priority = int.Parse(template.ValidPriorities[0]);
			summary.Severity = template.ValidSeverities[0];

			return summary;
		}
	}
}
