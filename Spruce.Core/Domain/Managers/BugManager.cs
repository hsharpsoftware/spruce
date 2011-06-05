using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class BugManager : WorkItemManager
	{
		public BugManager()
		{
			_andFilters.Add("[Work Item Type]='Bug'");
		}	
	}
}
