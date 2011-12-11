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
	/// <summary>
	/// The controller for all <see cref="IssueSummary"/> based actions.
	/// </summary>
	public class IssuesController : SpruceControllerBase
    {
		/// <summary>
		/// Displays a filterable, pageable and sortable list of <see cref="IssueSummary"/> objects.
		/// </summary>
		public override ActionResult Index(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			UpdateUserFilterOptions("issues:default");

			ListData data = FilterAndPage<IssueSummary>(GetIssueFilterOptions(), id, sortBy, desc, page, pageSize);
			return View(data);
		}

		/// <summary>
		/// Displays the form to enter a new Issue work item.
		/// </summary>
		[HttpGet]
		public ActionResult New(string id)
		{
			IssueManager manager = new IssueManager();
			IssueSummary summary = (IssueSummary)manager.NewItem();

			if (!string.IsNullOrWhiteSpace(id))
				summary.Title = id;

			MSAgileEditData<IssueSummary> data = new MSAgileEditData<IssueSummary>();
			data.PageTitle = "New issue";
			data.States = summary.ValidStates;
			data.Reasons = summary.ValidReasons;
			data.Priorities = summary.ValidPriorities;
			data.WorkItem = summary;

			return View("Edit", data);
		}

		/// <summary>
		/// Creates a new bug work item from the POST'd <see cref="IssueSummary"/>.
		/// </summary>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(IssueSummary item)
		{
			IssueManager manager = new IssueManager();

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
				IssueSummary summary = queryManager.ItemById<IssueSummary>(item.Id);
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

				MSAgileEditData<IssueSummary> data = new MSAgileEditData<IssueSummary>();
				data.WorkItem = summary;
				data.PageTitle = "Issue " + item.Id;
				data.States = summary.ValidStates;
				data.Reasons = summary.ValidReasons;
				data.Priorities = summary.ValidPriorities;

				return View(data);
			}
		}

		/// <summary>
		/// Edits an existing Issue work item, displaying a form to edit it.
		/// </summary>
		[HttpGet]
		public ActionResult Edit(int id, string fromUrl)
		{
			QueryManager manager = new QueryManager();
			IssueSummary item = manager.ItemById<IssueSummary>(id);
			item.IsNew = false;

			// Change the user's current project if this work item is different.
			// The project can be different if they've come from the stored queries page.
			if (item.ProjectName != UserContext.Current.CurrentProject.Name)
				UserContext.Current.ChangeCurrentProject(item.ProjectName);

			MSAgileEditData<IssueSummary> data = new MSAgileEditData<IssueSummary>();
			data.WorkItem = item;
			data.PageTitle = "Issue " + id;
			data.FromUrl = fromUrl;
			data.States = item.ValidStates;
			data.Reasons = item.ValidReasons;
			data.Priorities = item.ValidPriorities;

			return View(data);
		}

		/// <summary>
		/// Updates an existing Issue work item using the POST'd <see cref="BugSummary"/>.
		/// </summary>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(IssueSummary item, string fromUrl)
		{
			IssueManager manager = new IssueManager();

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
				IssueSummary summary = queryManager.ItemById<IssueSummary>(item.Id);
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

				MSAgileEditData<IssueSummary> data = new MSAgileEditData<IssueSummary>();
				data.WorkItem = summary;
				data.PageTitle = "Issue " + item.Id;
				data.FromUrl = fromUrl;
				data.States = summary.ValidStates;
				data.Reasons = summary.ValidReasons;
				data.Priorities = summary.ValidPriorities;

				return View(data);
			}
		}

		/// <summary>
		/// Downloads an Excel filename containing the current issues for the user's currently selected project.
		/// </summary>
		public ActionResult Excel()
		{
			ListData data = FilterAndPage<IssueSummary>(GetIssueFilterOptions(), "", "CreatedDate", true, 1, 10000);
			return Excel(data.WorkItems, "issues.xml");
		}

		/// <summary>
		/// Displays an RSS feed containing the current issues for the user's currently selected project.
		/// </summary>
		public ActionResult Rss(string projectName, string areaPath, string iterationPath, string filter)
		{
			ListData data = FilterAndPage<IssueSummary>(new FilterOptions(), projectName, "CreatedDate", true, 1, 10000);
			return Rss(data.WorkItems, "Issues", projectName, areaPath, iterationPath, filter);
		}

		/// <summary>
		/// Retrieves the filter options for the issue view.
		/// </summary>
		private FilterOptions GetIssueFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name)
				.GetByKey("issues:default");
		}

		/// <summary>
		/// Gets a new <see cref="IssueManager"/>
		/// </summary>
		protected override WorkItemManager GetManager()
		{
			return new IssueManager();
		}
    }
}
