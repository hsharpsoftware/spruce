using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	/// <summary>
	/// The values for the table column filters on list data.
	/// </summary>
	public class ListFilterValues
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

		public ListFilterValues()
		{
			PageSize = 1;
			CurrentPage = 1;
		}
	}
}
