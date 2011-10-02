using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spruce.Core
{
	public class ListData
	{
		public IEnumerable<WorkItemSummary> WorkItems { get; set; }
		public FilterValues FilterValues { get; set; }
		public string CurrentQuery { get; set; }

		public ListData()
		{
			FilterValues = new FilterValues();
			WorkItems = new List<WorkItemSummary>();
			CurrentQuery = "";
		}
	}

	public class FilterValues
	{
		public string Title { get; set; }
		public string AssignedTo { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public string Status { get; set; }
		public bool IsDescending { get; set; }
		public string SortBy { get; set; }

		public int CurrentPage { get; set; }
		public int PageCount { get; set; }
		public int PageSize { get; set; }

		public FilterValues()
		{
			PageSize = 1;
			CurrentPage = 1;
		}
	}
}
