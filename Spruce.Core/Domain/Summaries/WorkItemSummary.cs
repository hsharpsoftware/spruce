using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	[DebuggerDisplay("{Title}")]
	public class WorkItemSummary
	{
		public int Id { get; set; }
		public string ProjectName { get; set; }
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
		public FieldCollection Fields { get; internal set; }
		public AttachmentCollection Attachments { get; set; }
		public LinkCollection Links { get; set; }
		public RevisionCollection Revisions { get; set; }

		// For bugs
		public int? Priority { get; set; }
		public string Severity { get; set; }

		// For tasks
		public double EstimatedHours { get; set; }
		public double RemainingHours { get; set; }
		public double CompletedHours { get; set; }

		public bool IsBug { get; set; }

		public IList<string> ValidStates { get; set; }
		public IList<string> ValidPriorities { get; set; }
		public IList<string> ValidSeverities { get; set; }

		public WorkItemSummary()
		{
			ValidStates = new List<string>();
			ValidPriorities = new List<string>();
			ValidSeverities = new List<string>();
		}
	}
}