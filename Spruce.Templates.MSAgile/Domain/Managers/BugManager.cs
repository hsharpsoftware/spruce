using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class BugManager : WorkItemManager<BugSummary>
	{
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
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

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
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public override BugSummary NewItem()
		{
			BugSummary summary = new BugSummary();
			WorkItem item = base.CreateWorkItem(summary.WorkItemType, summary);

			summary.PopulateAllowedValues(item);
			summary.Priority = int.Parse(summary.ValidPriorities[0]);
			summary.Severity = summary.ValidSeverities[0];

			return summary;
		}
	}
}
