using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class TaskManager : WorkItemManager
	{
		public TaskManager()
		{
			_andFilters.Add("[Work Item Type]='Task'");
		}	
	}
}
