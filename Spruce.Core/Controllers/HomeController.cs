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
				SetHighlightedProject(id);

			// Dashboard
			return View("Index", DashboardManager.GetSummary());
		}

		public ActionResult SetProject(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetHighlightedProject(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetIteration(string id,string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetHighlightedIteration(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetArea(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetHighlightedArea(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetFilter(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			SetHighlightedFilter(id);

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
