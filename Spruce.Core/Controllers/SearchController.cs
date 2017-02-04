using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Spruce.Core.Search;

namespace Spruce.Core.Controllers
{
	/// <summary>
	/// The controller for searching in Spruce.
	/// </summary>
	public class SearchController : SpruceControllerBase
	{
		/// <summary>
		/// Searches TFS using the query provided by the 'q' parameter.
		/// </summary>
		public override ActionResult Index(string q, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			IList<WorkItemSummary> summaries = new List<WorkItemSummary>();
			ListData data = new ListData();

			if (!string.IsNullOrEmpty(q))
			{
				SearchManager manager = new SearchManager();

				if (manager.IsId(q))
				{
					// For single work item ids (that exist), redirect straight to their view page
					int id = int.Parse(q);
					QueryManager queryManager = new QueryManager();
					WorkItemSummary summary = queryManager.ItemById(id);

					if (summary != null)
					{
						return Redirect(SpruceSettings.SiteUrl + "/" + summary.Controller + "/View/" + id);
					}
				}
				else
				{
					data.WorkItems = manager.Search(q).ToList();
					PageList(data, sortBy, desc, page, pageSize);

					ViewData["search"] = q;
				}
			}

			return View(data);
		}
	}
}
