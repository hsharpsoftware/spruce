using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class TaskItemTemplate : WorkItemTemplate
	{
		public IList<string> ValidPriorities { get; set; }
		public IList<string> ValidSeverities { get; set; }

		public TaskItemTemplate() : base()
		{
			ValidPriorities = new List<string>();
			ValidSeverities = new List<string>();
		}

		public override void PopulateAllowedValues(WorkItem item)
		{
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