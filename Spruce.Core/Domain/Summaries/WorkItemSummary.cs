using System;
using System.Collections.Generic;

namespace Spruce.Core
{
	public class WorkItemSummary
	{
		public int Id { get; set; }
		public bool IsNew { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public string AreaName { get; set; }
		public string AreaPath { get; set; }
		public string IterationName { get; set; }
		public string IterationPath { get; set; }
		public string State { get; set; }
		public string AssignedTo { get; set; }
		public string ResolvedBy { get; set; }

		// For bugs
		public int? Priority { get; set; }

		// For tasks
		public int EstimatedHours { get; set; }
		public int RemainingHours { get; set; }
		public int CompletedHours { get; set; }

		public IList<string> ValidStates { get; set; }
		public IList<string> ValidPriorities { get; set; }

		public WorkItemSummary()
		{
			ValidStates = new List<string>();
			ValidStates = new List<string>();
		}
	}
}