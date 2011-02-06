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
		public ActionResult Index()
		{
			ControllerHelper.SetViewData(ViewData);

			// Dashboard
			return View("Index", WorkItemManager.AllBugs().ToList());
		}

		public ActionResult SetProject(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				id = Encoding.Default.GetString(Convert.FromBase64String(id));
				if (id != SpruceContext.Current.CurrentProject.Name)
				{
					SpruceContext.Current.SetProject(id);
				}
			}

			return RedirectToAction("Index");
		}

		public ActionResult SetIteration(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				id = Encoding.Default.GetString(Convert.FromBase64String(id));

				IterationSummary summary = SpruceContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == id);
				SpruceContext.Current.FilterSettings.IterationName = summary.Name;
				SpruceContext.Current.FilterSettings.IterationPath = summary.Path;
			}

			return RedirectToAction("Index");
		}

		public ActionResult SetArea(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				id = Encoding.Default.GetString(Convert.FromBase64String(id));

				AreaSummary summary = SpruceContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == id);
				SpruceContext.Current.FilterSettings.AreaName = summary.Name;
				SpruceContext.Current.FilterSettings.AreaPath = summary.Path;
			}

			return RedirectToAction("Index");
		}		
    }
}
