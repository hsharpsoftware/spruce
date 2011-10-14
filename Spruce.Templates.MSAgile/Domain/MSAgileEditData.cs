using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// Adds a list of severities and priorities to the base <see cref="EditData"/> class.
	/// </summary>
	/// <typeparam name="TWorkItemSummary">The type of the work item summary - either a BugSummary or TaskSummary.</typeparam>
	public class MSAgileEditData<TWorkItemSummary> : EditData<TWorkItemSummary> where TWorkItemSummary: WorkItemSummary,new()
	{
		public IEnumerable<string> Severities { get; set; }
		public IEnumerable<string> Priorities { get; set; }
	}
}
