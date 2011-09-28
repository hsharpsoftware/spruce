using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class MSAgileEditData<TWorkItemSummary> : EditData<TWorkItemSummary> where TWorkItemSummary: WorkItemSummary,new()
	{
		public IEnumerable<string> Severities { get; set; }
		public IEnumerable<string> Priorities { get; set; }
	}
}
