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
		public IEnumerable<WorkItemSummary> MyActiveBugs { get; set; }
		public IEnumerable<WorkItemSummary> MyActiveTasks { get; set; }
		public IEnumerable<ChangesetSummary> RecentCheckins { get; set; }
	}
}