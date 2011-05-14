using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public static class WorkItemExtensions
	{
		public static IList<string> ValidStates(this WorkItem workitem)
		{
			List<string> states = new List<string>();
			foreach (var item in workitem.Fields["State"].AllowedValues)
			{
				states.Add(item.ToString());
			}

			return states;
		}

		public static IList<string> ValidPriorities(this WorkItem workitem)
		{
			List<string> priorities = new List<string>();
			foreach (var item in workitem.Fields["Priority"].AllowedValues)
			{
				priorities.Add(item.ToString());
			}

			return priorities;
		}

		public static IList<WorkItem> ToList(this WorkItemCollection collection)
		{
			List<WorkItem> list = new List<WorkItem>();
			foreach (WorkItem item in collection)
			{
				list.Add(item);
			}

			return list;
		}
	}
}
