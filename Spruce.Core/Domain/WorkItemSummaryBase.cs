using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Spruce.Core
{
	[DebuggerDisplay("{Title}")]
	public class WorkItemSummaryBase
	{
		public int Id { get; set; }	
		public bool IsNew { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public DateTime ChangedDate { get; set; }
		public string ChangedBy { get; set; }

		public string Reason { get; set; }
		public string State { get; set; }
		public string AssignedTo { get; set; }
		public string ResolvedBy { get; set; }

		public string AreaName { get; set; }
		public string AreaPath { get; set; }
		public string IterationName { get; set; }
		public string IterationPath { get; set; }

		public string ProjectName { get; set; }

		

	}
}
