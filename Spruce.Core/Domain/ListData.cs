using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spruce.Core
{
	/// <summary>
	/// Provides data for views/controllers that display lists of work items.
	/// This includes the work item collections. (This is the MVC 'Model')
	/// </summary>
	public class ListData
	{
		/// <summary>
		/// The a <see cref="WorkItemSummary"/> collection for the data.
		/// </summary>
		public IEnumerable<WorkItemSummary> WorkItems { get; set; }

		/// <summary>
		/// The current filters that the <see cref="WorkItems"/> collection is being filtered with.
		/// </summary>
		public ListFilterValues FilterValues { get; set; }

		/// <summary>
		/// The WIQL query used to retrieve the the <see cref="WorkItems"/> collection.
		/// </summary>
		public string CurrentQuery { get; set; }

		public ListData()
		{
			FilterValues = new ListFilterValues();
			WorkItems = new List<WorkItemSummary>();
			CurrentQuery = "";
		}
	}
}
