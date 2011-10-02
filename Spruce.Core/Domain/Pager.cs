using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic;

namespace Spruce.Core
{
	/// <summary>
	/// Provides paging and sorting of an <see cref="IEnumerable"/>, updating the <see cref="PageCount"/> property.
	/// </summary>
	public class Pager
	{
		/// <summary>
		/// Read only. The number of pages the list contains, based on the <see cref="PageSize"/> property and the number
		/// of elements in the list.
		/// </summary>
		public int PageCount { get; private set; }

		/// <summary>
		/// Read only. The current page number for the page method.
		/// </summary>
		public int CurrentPageNumber { get; private set; }

		/// <summary>
		/// The number of items for each page.
		/// </summary>
		public int PageSize { get; set; }

		/// <summary>
		/// The column name for the type to sort on.
		/// </summary>
		public string SortBy { get; set; }

		/// <summary>
		/// Whether to sort descending or ascending.
		/// </summary>
		public bool Descending { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Pager"/> class.
		/// </summary>
		/// <param name="isHeatMap">if set to <c>true</c> the page being displayed is a heatmap and a Priority column is sorted..</param>
		/// <param name="sortBy">The column to sort by. If null, no sorting is performed.</param>
		/// <param name="descending">if set to <c>true</c> the list is sorted in descending order.</param>
		/// <param name="pageSize">The number of elements for each page.</param>
		public Pager(string sortBy, bool descending, int pageSize)
		{
			SortBy = sortBy;
			Descending = descending;
			PageSize = pageSize;
		}

		public virtual IEnumerable<T> Page<T>(IEnumerable<T> list, int pageNumber, string sortPrefix = "")
		{
			CurrentPageNumber = pageNumber;

			// Use dynamic linq for the sorting
			try
			{
				string sort = "";

				if (!string.IsNullOrEmpty(sortPrefix))
					sort = sortPrefix;

				if (!string.IsNullOrEmpty(SortBy))
				{
					if (!string.IsNullOrEmpty(sort))
						sort += ", ";

					string desc = (Descending) ? "desc" : "asc";
					sort += string.Format("{0} {1}", SortBy, desc);
				}

				list = list.AsQueryable().OrderBy(sort).AsEnumerable();
			}
			catch (ParseException)
			{
				// Ignore dynamic linq errors
			}

			//
			// Paging
			//
			if (PageSize < 1)
				PageSize = 5;

			if (pageNumber < 1)
				pageNumber = 1;

			// Number of items per page, rounded up
			int itemCount = list.Count();
			double itemsPerPage = list.Count() / (double) PageSize;
			PageCount = (int)Math.Ceiling(itemsPerPage);

			if (pageNumber > PageCount)
				pageNumber = PageCount;

			if (pageNumber >= PageCount)
			{
				// Grab everything all remaining items from the list
				list = list.Skip(PageSize * (pageNumber - 1));
			}
			else
			{
				// Skip past all items from the previous page.
				list = list.Skip(PageSize * (pageNumber - 1)).Take(PageSize);
			}

			return list;
		}
	}
}
