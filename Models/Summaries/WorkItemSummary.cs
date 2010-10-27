using System;
using System.Collections.Generic;

namespace Spruce.Models
{
	public class WorkItemSummary
	{
		public int Id { get; set; }
		public bool IsNew { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public string Area { get; set; }
		public string Iteration { get; set; }
		public string State { get; set; }
		public string AssignedTo { get; set; }
		public string ResolvedBy { get; set; }
		public int? Priority { get; set; }

		public IList<string> ValidStates { get; set; }

		public WorkItemSummary()
		{
			ValidStates = new List<string>();
		}
	}
}