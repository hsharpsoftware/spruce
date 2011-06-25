using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using Spruce.Core.Search;

namespace Spruce.Core.Controllers
{
	public class HomeController : ControllerBase
	{
		public ActionResult Index()
		{
			// Dashboard
			return View("Index", DashboardManager.GetSummary());
		}

		public ActionResult Changeset(int id)
		{
			return View(DashboardManager.GetChangeSet(id));
		}

		public ActionResult Search(string q)
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

		public ActionResult ChangeProject(string project, string area, string iteration, string fromUrl)
		{
			if (!string.IsNullOrEmpty(project) && project != UserContext.Current.CurrentProject.Name)
			{
				UserContext.Current.ChangeCurrentProject(project);
				UserContext.Current.UpdateSettings();
			}
			else
			{
				if (!string.IsNullOrEmpty(area))
				{
					AreaSummary summary = UserContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == area);
					UserContext.Current.Settings.AreaName = summary.Name;
					UserContext.Current.Settings.AreaPath = summary.Path;
					UserContext.Current.UpdateSettings();
				}

				if (!string.IsNullOrEmpty(iteration))
				{
					IterationSummary summary = UserContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iteration);
					UserContext.Current.Settings.IterationName = summary.Name;
					UserContext.Current.Settings.IterationPath = summary.Path;
					UserContext.Current.UpdateSettings();
				}
			}

			// Redirect
			if (!string.IsNullOrEmpty(fromUrl))
				return Redirect(fromUrl);
			else
				return RedirectToAction("Index");
		}

		public ActionResult StoredQueries()
		{
			ViewData["pageCount"] = 0;
			ViewData["currentPage"] = 1;
			ViewData["pageSize"] = 1;
			ViewData["desc"] = true;
			ViewData["CurrentQuery"] = Guid.Empty;
			return View(new List<WorkItemSummary>());
		}

		public ActionResult StoredQuery(Guid id, string sortBy, bool? desc, int? page, int? pageSize)
		{
			IEnumerable<WorkItemSummary> list = WorkItemManager.StoredQuery(id);
			list = PageList(list, false, sortBy, desc, page, pageSize);
			ViewData["CurrentQuery"] = id;

			return View("StoredQueries", list);
		}

		/// <summary>
		/// Returns a string containing Javascript 'constants' for the site.
		/// </summary>
		public ActionResult GlobalJsVars()
		{
			UrlHelper helper = new UrlHelper(HttpContext.Request.RequestContext);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("var SPRUCE_SCRIPTPATH = '{0}';", helper.Content("~/Assets/Scripts/")));
			builder.AppendLine(string.Format("var SPRUCE_CSSPATH = '{0}';", helper.Content("~/Assets/Css/")));
			builder.AppendLine(string.Format("var SPRUCE_IMAGEPATH = '{0}';", helper.Content("~/Assets/Images/")));

			return Content(builder.ToString(), "text/javascript");
		}
	}
}
