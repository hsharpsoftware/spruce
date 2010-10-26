using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spruce.Models;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
			WorkItemManager.New();
			return View(WorkItemManager.AllBugs().ToList());
        }

		public ActionResult AllItems()
		{
			WorkItemManager.Areas();
			return View("Index", WorkItemManager.AllItems().ToList());
		}

		public ActionResult Active()
		{
			return View("Index", WorkItemManager.AllActiveBugs().ToList());
		}

		public ActionResult Closed()
		{
			return View("Index", WorkItemManager.AllClosedBugs().ToList());
		}

		public ActionResult Tasks()
		{
			return View("AllTasks", WorkItemManager.AllTasks().ToList());
		}

		public ActionResult View(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			return View(item);
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			ViewData["Users"] = WorkItemManager.Users();

			WorkItemSummary item = WorkItemManager.ItemById(id);
			return View(item);
		}

		[HttpPost]
		public ActionResult Edit(WorkItemSummary item)
		{
			return View("Index");
		}
    }
}
