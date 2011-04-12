using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spruce.Core.Search;

namespace Spruce.Core.Controllers
{
	public class SearchController : ControllerBase
    {
        public ActionResult Index(string q)
        {
			IList<WorkItemSummary> summaries = new List<WorkItemSummary>();

			if (!string.IsNullOrEmpty(q))
			{
				SearchManager manager = new SearchManager();
				summaries = manager.Search(q);
				ViewData["search"] = q;

				if (manager.IsWorkItemId(q) && summaries.Count == 1)
				{
					// For single work item ids (that exist), redirect straight to the bug page
					return RedirectToAction("View", "Bugs", new { id = int.Parse(q) });
				}
			}

			return View(summaries);
        }
    }
}
