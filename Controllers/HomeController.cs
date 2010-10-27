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
			return View(WorkItemManager.AllBugs().ToList());
        }

		public ActionResult AllItems()
		{
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
		public ActionResult New()
		{
			ViewData["States"] = SpruceContext.Current.CurrentProject.AllowedStates;
			ViewData["Priorities"] = SpruceContext.Current.CurrentProject.AllowedPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;

			WorkItemSummary item = new WorkItemSummary();
			item.Priority = 1;
			item.Title = "Enter your title";
			item.Description = "Enter a brief description. See your project template for guidance";
			item.IsNew = true;
			return View("Edit",item);
		}

		[HttpPost]
		public ActionResult New(WorkItemSummary item)
		{
			// TODO: Validation
			item.CreatedBy = SpruceContext.Current.CurrentUser;
			item.IsNew = true;
			item.State = "Active";
			WorkItemManager.SaveBug(item);

			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			item.IsNew = false;

			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = SpruceContext.Current.CurrentProject.AllowedPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;
		
			return View(item);
		}

		[HttpPost]
		public ActionResult Edit(WorkItemSummary item)
		{
			WorkItemManager.SaveBug(item);
			return RedirectToAction("Index");
		}
    }
}
