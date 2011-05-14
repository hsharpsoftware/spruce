using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using System.Linq.Dynamic;

namespace Spruce.Core.Controllers
{
	public class BugsController : ControllerBase
    {
		public ActionResult Index(string id, string sortBy, bool? desc,int? page,int? pageSize)
		{
			SetBugView("Index");

			int pageCount = 1;
			int currentPage = page.HasValue ? page.Value : 1;
			int pageSizeVal = pageSize.HasValue ? pageSize.Value : 10;
			IEnumerable<WorkItemSummary> list = GetList(id, false, sortBy, desc, out pageCount, pageSizeVal, currentPage);

			if (desc.HasValue)
				ViewData["desc"] = desc;
			else
				ViewData["desc"] = true;

			ViewData["pageCount"] = pageCount;
			ViewData["currentPage"] = currentPage;
			ViewData["pageSize"] = pageSizeVal;

			return View(list);
		}

		public ActionResult Heatmap(string id, string sortBy, bool? desc, int? page, int? pageSize)
		{
			SetBugView("Heatmap");

			int pageCount = 1;
			int currentPage = page.HasValue ? page.Value : 1;
			int pageSizeVal = pageSize.HasValue ? pageSize.Value : 10;
			IEnumerable<WorkItemSummary> list = GetList(id, true, sortBy, desc, out pageCount, pageSizeVal, currentPage);

			if (desc.HasValue)
				ViewData["desc"] = desc;
			else
				ViewData["desc"] = true;

			ViewData["pageCount"] = pageCount;
			ViewData["currentPage"] = currentPage;
			ViewData["pageSize"] = pageSizeVal;

			return View(list);
		}

		private IEnumerable<WorkItemSummary> GetList(string projectName,bool isHeatMap, string sortBy,bool? descending,out int pageCount,int pageSize,int pageNumber)
		{
			if (!string.IsNullOrEmpty(projectName))
				SetHighlightedProject(projectName);

			IEnumerable<WorkItemSummary> list;

			switch (SpruceContext.Current.UserSettings.FilterType)
			{
				case FilterType.Active:
					list = WorkItemManager.AllActiveBugs();
					break;

				case FilterType.Resolved:
					list = WorkItemManager.AllResolvedBugs();
					break;

				case FilterType.Closed:
					list = WorkItemManager.AllClosedBugs();
					break;

				case FilterType.AssignedToMe:
					list = WorkItemManager.BugsAssignedToMe();
					break;

				case FilterType.Today:
					list = WorkItemManager.AllBugs();
					break;

				case FilterType.Yesterday:
					list = WorkItemManager.AllBugs();
					break;

				case FilterType.ThisWeek:
					list = WorkItemManager.AllBugs();
					break;

				case FilterType.All:
				default:
					list = WorkItemManager.AllBugs();
					break;
			}

			// Use dynamic linq for the sorting
			try
			{
				string sort = "";
				if (isHeatMap)
				{
					sort = "Priority asc";
				}

				if (!string.IsNullOrEmpty(sortBy))
				{
					if (!string.IsNullOrEmpty(sort))
						sort += ", ";

					string desc = (descending == true) ? "desc" : "asc";
					sort += string.Format("{0} {1}", sortBy, desc);	
				}

				list = list.AsQueryable().OrderBy(sort).AsEnumerable();
			}
			catch (ParseException)
			{
				// Ignore dynamic linq errors
			}

			//
			// Paging
			//
			if (pageSize < 1)
				pageSize = 5;

			if (pageNumber < 1)
				pageNumber = 1;

			// Number of items per page, rounded up
			int itemCount = list.Count();
			double itemsPerPage = list.Count() / (double) pageSize;
			pageCount = (int) Math.Ceiling(itemsPerPage);

			if (pageNumber > pageCount)
				pageNumber = pageCount;

			if (pageNumber >= pageCount)
			{
				// Grab everything all remaining items from the list
				list = list.Skip(pageSize * (pageNumber - 1));
			}
			else
			{
				// Skip past all items from the previous page.
				list = list.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
			}

			return list;
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

		public ActionResult Active()
		{
			Session["ListLink"] = "Active";
			return View("Index", WorkItemManager.AllActiveBugs().ToList());
		}

		public ActionResult Closed()
		{
			Session["ListLink"] = "Closed";
			return View("Index", WorkItemManager.AllClosedBugs().ToList());
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
		public ActionResult New(string id)
		{
			WorkItemSummary item = WorkItemManager.NewBug();

			if (!string.IsNullOrWhiteSpace(id))
				item.Title = id;

			ViewData["PageName"] = "New bug";
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
				WorkItemManager.SaveBug(item);

				// Set the project/iteration/area to the previously edited item
				SetHighlightedProject(item.ProjectName);
				SetHighlightedArea(item.AreaPath);
				SetHighlightedIteration(item.IterationPath);

				return RedirectToAction("Index");
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;
				return RedirectToAction("New", new { id = item.Title});
			}
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
