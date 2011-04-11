using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Spruce.Core
{
	public class DashboardSummary
	{
		public int BugCount { get; set; }
		public int TaskCount { get; set; }
		public int ActiveBugs { get; set; }
		public int ActiveTasks { get; set; }
		public int MyActiveBugCount { get; set; }
		public List<WorkItemSummary> MyActiveBugs { get; set; }
		public List<WorkItemSummary> MyActiveTasks { get; set; }
		public List<ChangesetSummary> RecentCheckins { get; set; }
	}
}