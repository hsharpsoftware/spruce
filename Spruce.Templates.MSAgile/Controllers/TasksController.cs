using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Text;
using System.IO;
using System.ServiceModel.Syndication;
using Spruce.Core.Controllers;
using Spruce.Core;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// The controller for all <see cref="TaskSummary"/> based actions.
	/// </summary>
	public class TasksController : SpruceControllerBase
    {
		/// <summary>
		/// Displays a filterable, pageable and sortable list of <see cref="TaskSummary"/> objects.
		/// </summary>
		public override ActionResult Index(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			UpdateUserFilterOptions("tasks:default");

			ListData data = FilterAndPage<TaskSummary>(GetTaskFilterOptions(), id, sortBy, desc, page, pageSize);
			return View(data);
		}

		/// <summary>
		/// Displays the form to enter a new Task work item.
		/// </summary>
		[HttpGet]
		public ActionResult New(string id)
		{
			TaskManager manager = new TaskManager();
			TaskSummary summary = (TaskSummary)manager.NewItem();

			if (!string.IsNullOrWhiteSpace(id))
				summary.Title = id;

			MSAgileEditData<TaskSummary> data = new MSAgileEditData<TaskSummary>();
			data.PageTitle = "New task";
			data.States = summary.ValidStates;
			data.Reasons = summary.ValidReasons;
			data.Priorities = summary.ValidPriorities;
			data.Severities = summary.ValidSeverities;
			data.WorkItem = summary;

			return View("Edit", data);
		}

		/// <summary>
		/// Creates a new bug work item from the POST'd <see cref="TaskSummary"/>.
		/// </summary>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(TaskSummary item)
		{
			TaskManager manager = new TaskManager();

			try
			{
				item.CreatedBy = UserContext.Current.Name;
				item.IsNew = true;
				manager.Save(item); // item.Id is updated

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

				// Get the original back, to populate the valid reasons.
				QueryManager queryManager = new QueryManager();
				TaskSummary summary = queryManager.ItemById<TaskSummary>(item.Id);
				summary.IsNew = false;

				// Repopulate from the POST'd data
				summary.Title = item.Title;
				summary.State = item.State;
				summary.Reason = item.Reason;
				summary.Priority = item.Priority;
				summary.Description = item.Description;
				summary.AssignedTo = item.AssignedTo;
				summary.AreaId = item.AreaId;
				summary.AreaPath = item.AreaPath;
				summary.IterationId = item.IterationId;
				summary.IterationPath = item.IterationPath;

				MSAgileEditData<TaskSummary> data = new MSAgileEditData<TaskSummary>();
				data.WorkItem = summary;
				data.PageTitle = "Task " + item.Id;
				data.States = summary.ValidStates;
				data.Reasons = summary.ValidReasons;
				data.Priorities = summary.ValidPriorities;
				data.Severities = summary.ValidSeverities;

				return View(data);
			}
		}

		/// <summary>
		/// Edits an existing Task work item, displaying a form to edit it.
		/// </summary>
		[HttpGet]
		public ActionResult Edit(int id, string fromUrl)
		{
			QueryManager manager = new QueryManager();
			TaskSummary item = manager.ItemById<TaskSummary>(id);
			item.IsNew = false;

			// Change the user's current project if this work item is different.
			// The project can be different if they've come from the stored queries page.
			if (item.ProjectName != UserContext.Current.CurrentProject.Name)
				UserContext.Current.ChangeCurrentProject(item.ProjectName);

			MSAgileEditData<TaskSummary> data = new MSAgileEditData<TaskSummary>();
			data.WorkItem = item;
			data.PageTitle = "Task " + id;
			data.FromUrl = fromUrl;
			data.States = item.ValidStates;
			data.Reasons = item.ValidReasons;
			data.Priorities = item.ValidPriorities;
			data.Severities = item.ValidSeverities;

			return View(data);
		}

		/// <summary>
		/// Updates an existing Task work item using the POST'd <see cref="BugSummary"/>.
		/// </summary>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(TaskSummary item, string fromUrl)
		{
			TaskManager manager = new TaskManager();

			try
			{
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

				if (string.IsNullOrEmpty(fromUrl))
					return RedirectToAction("View", new { id = item.Id });
				else
					return Redirect(fromUrl);
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;

				// Get the original back, to populate the valid reasons.
				QueryManager queryManager = new QueryManager();
				TaskSummary summary = queryManager.ItemById<TaskSummary>(item.Id);
				summary.IsNew = false;

				// Repopulate from the POST'd data
				summary.Title = item.Title;
				summary.State = item.State;
				summary.Reason = item.Reason;
				summary.Priority = item.Priority;
				summary.Description = item.Description;
				summary.AssignedTo = item.AssignedTo;
				summary.AreaId = item.AreaId;
				summary.AreaPath = item.AreaPath;
				summary.IterationId = item.IterationId;
				summary.IterationPath = item.IterationPath;

				MSAgileEditData<TaskSummary> data = new MSAgileEditData<TaskSummary>();
				data.WorkItem = summary;
				data.PageTitle = "Task " + item.Id;
				data.FromUrl = fromUrl;
				data.States = summary.ValidStates;
				data.Reasons = summary.ValidReasons;
				data.Priorities = summary.ValidPriorities;
				data.Severities = summary.ValidSeverities;

				return View(data);
			}
		}

		/// <summary>
		/// Downloads an Excel filename containing the current tasks for the user's currently selected project.
		/// </summary>
		public ActionResult Excel()
		{
			ListData data = FilterAndPage<TaskSummary>(GetTaskFilterOptions(), "", "CreatedDate", true, 1, 10000);
			return Excel(data.WorkItems, "tasks.xml");
		}

		/// <summary>
		/// Displays an RSS feed containing the current tasks for the user's currently selected project.
		/// </summary>
		public ActionResult Rss(string projectName, string areaPath, string iterationPath, string filter)
		{
			ListData data = FilterAndPage<TaskSummary>(new FilterOptions(), projectName, "CreatedDate", true, 1, 10000);
			return Rss(data.WorkItems, "Tasks", projectName, areaPath, iterationPath, filter);
		}

		/// <summary>
		/// Retrieves the filter options for the task view.
		/// </summary>
		private FilterOptions GetTaskFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name)
				.GetByKey("tasks:default");
		}

		/// <summary>
		/// Gets a new <see cref="TaskManager"/>
		/// </summary>
		protected override WorkItemManager GetManager()
		{
			return new TaskManager();
		}
    }
}
