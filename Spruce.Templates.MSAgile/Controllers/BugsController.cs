using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using System.Linq.Dynamic;
using System.IO;
using System.ServiceModel.Syndication;
using Spruce.Core.Controllers;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class BugsController : SpruceControllerBase<BugSummary>
    {
		public override ActionResult Index(string id, string sortBy, bool? desc,int? page,int? pageSize,
			string title,string assignedTo,string startDate,string endDate,string status)
		{
			SetBugView("Index");
			UpdateUserFilterOptions();

			ListData<BugSummary> data = FilterAndPage(GetBugFilterOptions(), id, false, sortBy, desc, page, pageSize);

			return View(data);
		}

		public ActionResult Heatmap(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			SetBugView("Heatmap");
			UpdateUserFilterOptions();

			ListData<BugSummary> data = FilterAndPage(GetBugHeatmapFilterOptions(), id, true, sortBy, desc, page, pageSize);

			return View(data);
		}


		[HttpGet]
		public ActionResult New(string id)
		{
			BugManager manager = new BugManager();
			BugSummary summary = manager.NewItem();

			if (!string.IsNullOrWhiteSpace(id))
				summary.Title = id;

			MSAgileEditData<BugSummary> data = new MSAgileEditData<BugSummary>();
			data.PageTitle = "New bug";
			data.States = summary.ValidStates;
			data.Reasons = summary.ValidReasons;
			data.Priorities = summary.ValidPriorities;
			data.Severities = summary.ValidSeverities;
			data.WorkItem = summary;

			return View("Edit", data);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(WorkItemSummary item)
		{
			BugManager manager = new BugManager();

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
				return RedirectToAction("New", new { id = item.Title});
			}
		}

		[HttpGet]
		public ActionResult Edit(int id,string fromUrl)
		{
			QueryManager manager = new QueryManager();
			BugSummary item = manager.ItemById<BugSummary>(id);
			item.IsNew = false;

			ViewData["fromUrl"] = fromUrl;
			ViewData["PageName"] = "Bug " + id;

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

			return View(item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(WorkItemSummary item,string fromUrl)
		{
			BugManager manager = new BugManager();

			try
			{
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
				// Get the original back, to populate the valid reasons.
				QueryManager queryManager = new QueryManager();
				BugSummary summary = queryManager.ItemById<BugSummary>(item.Id);

				// If any error occurs, all the values previous selected aren't shown.
				// This is from a shortcoming with the WorkItemSummary being POST'd 
				// missing some of the fields, e.g. ValidReasons.
				TempData["Error"] = e.Message;
				//return RedirectToAction("Edit", new { id = item.Id});

				ViewData["fromUrl"] = fromUrl;
				ViewData["PageName"] = "Bug " + item.Id;

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
				return View(item);
			}
		}

		public ActionResult Excel()
		{
			ListData<BugSummary> data = FilterAndPage(GetBugFilterOptions(), "", true, "CreatedDate", true, 1, 10000);
			return Excel(data.WorkItems);
		}

		public ActionResult Rss(string projectName, string areaPath,string iterationPath,string filter)
		{
			ListData<BugSummary> data = FilterAndPage(new FilterOptions(), projectName, true, "CreatedDate", true, 1, 10000);
			return Rss(data.WorkItems, "Bugs", projectName, areaPath, iterationPath, filter);
		}

		public ActionResult GetReasonsForState(string state)
		{
			QueryManager manager = new QueryManager();
			return Json(manager.GetAllowedValuesForState<BugSummary>(state, "Reason"), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Sets the user settings for either heatmap or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		private void SetBugView(string actionName)
		{
			UserContext.Current.Settings.BugView = actionName;
		}

		private FilterOptions GetBugFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name).BugFilterOptions;
		}

		private FilterOptions GetBugHeatmapFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name).BugHeatmapFilterOptions;
		}

		protected override WorkItemManager<BugSummary> GetManager()
		{
			return new BugManager();
		}
    }
}
