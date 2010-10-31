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

		public ActionResult Create1000()
		{
			for (int i = 0; i < 1000; i++)
			{
				WorkItemSummary summary = WorkItemManager.NewBug();
				summary.Area = SpruceContext.Current.FilterSettings.AreaPath;
				summary.Iteration = SpruceContext.Current.FilterSettings.IterationPath;
				summary.Title = "Generated item " + i;
				summary.Description = "Generated description " + i;
				WorkItemManager.SaveBug(summary);
			}

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

		public ActionResult AllTasks()
		{
			return View("AllTasks", WorkItemManager.AllTasks().ToList());
		}

		public ActionResult View(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			return View(item);
		}

		public ActionResult SaveSettings(string settingsProject,string settingsIteration,string settingsArea,string settingsStates)
		{
			if (settingsProject != SpruceContext.Current.CurrentProject.Name)
			{
				SpruceContext.Current.SetProject(settingsProject);
			}

			FilterSettings settings = SpruceContext.Current.FilterSettings;
			settings.AreaPath = settingsArea;
			settings.IterationPath = settingsIteration;
			settings.States = settingsStates;

			return RedirectToAction("Index");
		}

		public ActionResult Resolve(int id)
		{
			WorkItemManager.Resolve(id);
			return RedirectToAction("View", new { id = id });
		}

		public ActionResult Close(int id)
		{
			WorkItemManager.Close(id);
			return RedirectToAction("View", new { id = id });
		}

		[HttpGet]
		public ActionResult NewBug(string id)
		{
			WorkItemSummary item = WorkItemManager.NewBug();

			if (!string.IsNullOrWhiteSpace(id))
				item.Title = id;

			ViewData["Message"] = "New bug";
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;
			
			return View("Edit",item);
		}

		[HttpPost]
		public ActionResult NewBug(WorkItemSummary item)
		{
			item.CreatedBy = SpruceContext.Current.CurrentUser;
			item.IsNew = true;
			WorkItemManager.SaveBug(item);

			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult NewTask()
		{
			WorkItemSummary item = WorkItemManager.NewTask();

			ViewData["Message"] = "New task";
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;

			return View("Edit", item);
		}

		[HttpPost]
		public ActionResult NewTask(WorkItemSummary item)
		{
			item.CreatedBy = SpruceContext.Current.CurrentUser;
			item.IsNew = true;
			WorkItemManager.SaveTask(item);

			return RedirectToAction("AllTasks");
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			item.IsNew = false;

			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;
		
			return View(item);
		}

		[HttpPost]
		public ActionResult Edit(WorkItemSummary item)
		{
			WorkItemManager.SaveExisting(item);
			return RedirectToAction("Index");
		}
    }
}
