using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using Spruce.Core.Search;
using Spruce.Core.Controllers;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class DashboardController : Controller
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
	}
}
