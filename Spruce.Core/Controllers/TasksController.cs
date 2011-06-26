using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using System.IO;
using System.ServiceModel.Syndication;

namespace Spruce.Core.Controllers
{
	public class TasksController : ControllerBase
    {
		public ActionResult Index(string id, string sortBy, bool? desc, int? page, int? pageSize)
		{
			IEnumerable<WorkItemSummary> list = FilterAndPageList(id, true, sortBy, desc, page, pageSize, new TaskManager());
			return View(list);
		}

		public ActionResult View(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
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
			ViewData["Severities"] = item.ValidSeverities;
			ViewData["Users"] = UserContext.Current.Users;

			return View("Edit", item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(WorkItemSummary item)
		{
			try
			{
				item.CreatedBy = UserContext.Current.Name;
				item.IsNew = true;
				WorkItemManager.SaveTask(item);

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

						WorkItemManager.SaveAttachments(item.Id, attachments);
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

			ViewData["PageName"] = "Task " + id;
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
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
				WorkItemManager.SaveExisting(item);

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

						WorkItemManager.SaveAttachments(item.Id, attachments);
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

		private string SaveFile(string fieldName, int id)
		{
			string filename = Request.Files[fieldName].FileName;
			if (!string.IsNullOrEmpty(filename))
			{
				string directory = string.Format(@"{0}{1}", SpruceSettings.UploadDirectory, id);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string filePath = string.Format(@"{0}\{1}", directory, filename);
				HttpPostedFileBase postedFile = Request.Files[fieldName] as HttpPostedFileBase;
				postedFile.SaveAs(filePath);

				return filePath;
			}
			else
			{
				return "";
			}
		}

		public ActionResult Excel()
		{
			IEnumerable<WorkItemSummary> list = FilterAndPageList("", true, "CreatedDate", true, 1, 10000,new TaskManager());

			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				ViewData.Model = list;
				ViewEngineResult engineResult = ViewEngines.Engines.FindView(ControllerContext, "Excel", "_ExcelLayout");
				ViewContext context = new ViewContext(ControllerContext, engineResult.View, ViewData, TempData, writer);

				engineResult.View.Render(context, writer);
				writer.Close();

				FileContentResult result = new FileContentResult(Encoding.Default.GetBytes(builder.ToString()), "application/ms-excel");
				result.FileDownloadName = "tasks.xml";

				return result;
			}
		}

		public ActionResult Rss(string projectName, string areaPath, string iterationPath, string filter)
		{
			if (!string.IsNullOrEmpty(areaPath))
			{
				areaPath = HttpUtility.UrlDecode(areaPath);
				AreaSummary summary = UserContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == areaPath);
				UserContext.Current.Settings.AreaName = summary.Name;
				UserContext.Current.Settings.AreaPath = summary.Path;
				UserContext.Current.UpdateSettings();
			}
			else if (!string.IsNullOrEmpty(iterationPath))
			{
				areaPath = HttpUtility.UrlDecode(iterationPath);
				IterationSummary summary = UserContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iterationPath);
				UserContext.Current.Settings.IterationName = summary.Name;
				UserContext.Current.Settings.IterationPath = summary.Path;
				UserContext.Current.UpdateSettings();
			}

			IEnumerable<WorkItemSummary> list = FilterAndPageList(projectName, true, "CreatedDate", true, 1, 10000,new TaskManager());

			RssActionResult result = new RssActionResult();
			result.Feed = GetRssFeed(list, "Tasks");
			return result;
		}
    }
}
