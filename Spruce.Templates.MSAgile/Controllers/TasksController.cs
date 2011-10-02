using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using System.IO;
using System.ServiceModel.Syndication;
using Spruce.Core.Controllers;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class TasksController : SpruceControllerBase
    {
		public override ActionResult Index(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			UpdateUserFilterOptions("tasks:default");

			ListData data = FilterAndPage<TaskSummary>(GetTaskFilterOptions(), id, sortBy, desc, page, pageSize);
			return View(data);
		}

		[HttpGet]
		public ActionResult New(string id)
		{
			TaskManager manager = new TaskManager();
			TaskSummary item = (TaskSummary) manager.NewItem();

			if (!string.IsNullOrWhiteSpace(id))
				item.Title = id;

			ViewData["PageName"] = "New task";

			//ViewData["PageName"] = "New bug";
			//ViewData["States"] = item.ValidStates;
			//ViewData["Reasons"] = item.ValidReasons;
			//ViewData["Priorities"] = item.ValidPriorities;
			//ViewData["Severities"] = item.ValidSeverities;

			ViewData["PageName"] = "New bug";
			ViewData["States"] = new List<string>();
			ViewData["Reasons"] = new List<string>();
			ViewData["Priorities"] = new List<string>();
			ViewData["Severities"] = new List<string>();
			ViewData["Users"] = UserContext.Current.Users;

			return View("Edit", item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(WorkItemSummary item)
		{
			TaskManager manager = new TaskManager();

			try
			{
				item.CreatedBy = UserContext.Current.Name;
				item.IsNew = true;
				manager.Save(item);

				// Save the files once it's saved (as we can't add an AttachmentsCollection as it has no constructors)
				if (Request.Files.Count > 0)
				{
					try
					{
						// Save to the App_Data folder.
						List<Attachment> attachments = new List<Attachment>();
						string filename1 = SaveFile("uploadFile1", item.Id);
						string filename2 = SaveFile("uploadFile2", item.Id);
						string filename3 = SaveFile("uploadFile3", item.Id);

						if (!string.IsNullOrEmpty(filename1))
						{
							attachments.Add(new Attachment(filename1, Request.Form["uploadFile1Comments"]));
						}
						if (!string.IsNullOrEmpty(filename2))
						{
							attachments.Add(new Attachment(filename2, Request.Form["uploadFile2Comments"]));
						}
						if (!string.IsNullOrEmpty(filename3))
						{
							attachments.Add(new Attachment(filename3, Request.Form["uploadFile3Comments"]));
						}

						manager.SaveAttachments(item.Id, attachments);
					}
					catch (IOException e)
					{
						TempData["Error"] = e.Message;
						return RedirectToAction("Edit", new { id = item.Id });
					}
				}

				return RedirectToAction("Index");
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;
				return RedirectToAction("New", new { id = item.Title });
			}
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			QueryManager manager = new QueryManager();
			TaskSummary item = manager.ItemById<TaskSummary>(id);
			item.IsNew = false;

			ViewData["PageName"] = "Task " + id;

			//ViewData["PageName"] = "New bug";
			//ViewData["States"] = item.ValidStates;
			//ViewData["Reasons"] = item.ValidReasons;
			//ViewData["Priorities"] = item.ValidPriorities;
			//ViewData["Severities"] = item.ValidSeverities;

			ViewData["PageName"] = "New bug";
			ViewData["States"] = new List<string>();
			ViewData["Reasons"] = new List<string>();
			ViewData["Priorities"] = new List<string>();
			ViewData["Severities"] = new List<string>();
			ViewData["Users"] = UserContext.Current.Users;
			ViewData["Error"] = TempData["Error"];

			return View(item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(WorkItemSummary item)
		{
			try
			{
				TaskManager manager = new TaskManager();
				item.IsNew = false;
				manager.Save(item);

				// Save the files once it's saved (as we can't add an AttachmentsCollection as it has no constructors)
				if (Request.Files.Count > 0)
				{
					try
					{
						// Save to the App_Data folder.
						List<Attachment> attachments = new List<Attachment>();
						string filename1 = SaveFile("uploadFile1", item.Id);
						string filename2 = SaveFile("uploadFile2", item.Id);
						string filename3 = SaveFile("uploadFile3", item.Id);

						if (!string.IsNullOrEmpty(filename1))
						{
							attachments.Add(new Attachment(filename1, Request.Form["uploadFile1Comments"]));
						}
						if (!string.IsNullOrEmpty(filename2))
						{
							attachments.Add(new Attachment(filename2, Request.Form["uploadFile2Comments"]));
						}
						if (!string.IsNullOrEmpty(filename3))
						{
							attachments.Add(new Attachment(filename3, Request.Form["uploadFile3Comments"]));
						}

						manager.SaveAttachments(item.Id, attachments);
					}
					catch (IOException e)
					{
						TempData["Error"] = e.Message;
						return RedirectToAction("Edit", new { id = item.Id });
					}
				}

				return RedirectToAction("View", new { id = item.Id });
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;
				return RedirectToAction("Edit", new { id = item.Id});
			}
		}

		public ActionResult Excel()
		{
			ListData data = FilterAndPage<TaskSummary>(GetTaskFilterOptions(), "", "CreatedDate", true, 1, 10000);
			return Excel(data.WorkItems);
		}

		public ActionResult Rss(string projectName, string areaPath, string iterationPath, string filter)
		{
			ListData data = FilterAndPage<TaskSummary>(new FilterOptions(), projectName, "CreatedDate", true, 1, 10000);
			return Rss(data.WorkItems, "Tasks", projectName, areaPath, iterationPath, filter);
		}

		private FilterOptions GetTaskFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name)
				.GetByKey("tasks:default");
		}

		protected override WorkItemManager GetManager()
		{
			return new TaskManager();
		}
    }
}
