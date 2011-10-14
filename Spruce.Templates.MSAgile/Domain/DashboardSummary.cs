using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// This is the Model for the project dashboard, and includes information about current bugs, tasks and checkins.
	/// </summary>
	public class DashboardSummary
	{
		public int BugCount { get; set; }
		public int TaskCount { get; set; }
		public int ActiveBugCount { get; set; }
		public int ActiveTaskCount { get; set; }
		public int MyActiveBugCount { get; set; }
		public int MyActiveTaskCount { get; set; }
		public int RecentCheckinCount { get; set; }
		public IEnumerable<WorkItemSummary> MyActiveBugs { get; set; }
		public IEnumerable<WorkItemSummary> MyActiveTasks { get; set; }
		public IEnumerable<ChangesetSummary> RecentCheckins { get; set; }
	}
}