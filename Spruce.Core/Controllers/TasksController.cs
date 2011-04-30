using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;

namespace Spruce.Core.Controllers
{
	public class TasksController : ControllerBase
    {
		public ActionResult Index(string id)
		{
			SetTaskView("Index");
			return View(GetList(id));
		}

		public ActionResult List(string id)
		{
			SetTaskView("List");
			return View(GetList(id));
		}

		private IEnumerable<WorkItemSummary> GetList(string projectName)
		{
			if (!string.IsNullOrEmpty(projectName))
				SetHighlightedProject(projectName);

			switch (SpruceContext.Current.UserSettings.FilterType)
			{
				case FilterType.Active:
					return WorkItemManager.AllTasks();

				case FilterType.Resolved:
					return WorkItemManager.AllTasks();

				case FilterType.Closed:
					return WorkItemManager.AllTasks();

				case FilterType.AssignedToMe:
					return WorkItemManager.AllTasks();

				case FilterType.Today:
					return WorkItemManager.AllTasks();

				case FilterType.Yesterday:
					return WorkItemManager.AllTasks();

				case FilterType.ThisWeek:
					return WorkItemManager.AllTasks();

				case FilterType.All:
				default:
					return WorkItemManager.AllTasks();
			}
		}

		public ActionResult View(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);

			if (TempData["RedirectedFromHomeController"] == null)
			{
				// Only set these if the user hasn't previously just clicked the right side area/iteration/project
				SetHighlightedProject(item.ProjectName);
				SetHighlightedArea(item.AreaPath);
				SetHighlightedIteration(item.IterationPath);
			}

			return View(item);
		}

		[HttpGet]
		public ActionResult New(string id)
		{
			WorkItemSummary item = WorkItemManager.NewTask();

			if (!string.IsNullOrWhiteSpace(id))
				item.Title = id;

			ViewData["PageName"] = "New task";
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;

			return View("Edit", item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(WorkItemSummary item)
		{
			try
			{
				item.CreatedBy = SpruceContext.Current.CurrentUser;
				item.IsNew = true;
				WorkItemManager.SaveTask(item);

				// Set the project/iteration/area to the previously edited item
				SetHighlightedProject(item.ProjectName);
				SetHighlightedArea(item.AreaPath);
				SetHighlightedIteration(item.IterationPath);

				return RedirectToAction("Index");
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;
				return RedirectToAction("New", new { id = item.Title });
			}
		}

		public ActionResult Close(int id)
		{
			WorkItemManager.Close(id);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			item.IsNew = false;

			ViewData["PageName"] = "Bug " + id;
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;
			ViewData["Error"] = TempData["Error"];

			return View(item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(WorkItemSummary item)
		{
			try
			{
				WorkItemManager.SaveExisting(item);

				// Set the project/iteration/area to the previously edited item
				SetHighlightedProject(item.ProjectName);
				SetHighlightedArea(item.AreaPath);
				SetHighlightedIteration(item.IterationPath);

				return RedirectToAction("View", new { id = item.Id });
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;
				return RedirectToAction("Edit", new { id = item.Id});
			}
		}
    }
}
