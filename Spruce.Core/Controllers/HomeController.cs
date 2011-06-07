using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;

namespace Spruce.Core.Controllers
{
	public class HomeController : ControllerBase
    {
		public ActionResult Index(string id)
		{
			if (!string.IsNullOrEmpty(id))
				SetProject(id);

			// Dashboard
			return View("Index", DashboardManager.GetSummary());
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

		public ActionResult StoredQuery(Guid id, string sortBy, bool? desc,int? page,int? pageSize)
		{
			IEnumerable<WorkItemSummary> list = WorkItemManager.StoredQuery(id);
			list = PageList(list, false, sortBy, desc, page, pageSize);
			ViewData["CurrentQuery"] = id;
			
			return View("StoredQueries",list);
		}

		public ActionResult Projects()
		{
			return View(UserContext.Current.ProjectNames);
		}

		public ActionResult SetCurrentProject(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				id = id.FromBase64();
				SetProject(id);
			}

			return View("Projects", UserContext.Current.ProjectNames);
		}

		public ActionResult Changeset(int id)
		{
			return View(DashboardManager.GetChangeSet(id));
		}

		public ActionResult SetIteration(string id,string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetIteration(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetArea(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetArea(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetFilter(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;

			return Redirect(fromUrl);
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
