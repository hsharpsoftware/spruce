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

namespace Spruce.Core.Controllers
{
	public class BugsController : ControllerBase
    {
		public ActionResult Index(string id, string sortBy, bool? desc,int? page,int? pageSize,
			string title,string assignedTo,string startDate,string endDate,string status)
		{
			SetBugView("Index");

			if (Request.QueryString.Count > 0)
				UserContext.Current.Settings.UpdateBugFilterOptions(UserContext.Current.CurrentProject.Name,title, assignedTo, startDate, endDate, status);

			IEnumerable<WorkItemSummary> list = FilterAndPageList(GetBugFilterOptions(), id, false, sortBy, desc, page, pageSize, new BugManager());

			return View(list);
		}

		public ActionResult Heatmap(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			SetBugView("Heatmap");

			if (Request.QueryString.Count > 0)
				UserContext.Current.Settings.UpdateBugHeatmapFilterOptions(UserContext.Current.CurrentProject.Name, title, assignedTo, startDate, endDate, status);

			IEnumerable<WorkItemSummary> list = FilterAndPageList(GetBugHeatmapFilterOptions(), id, true, sortBy, desc, page, pageSize, new BugManager());

			return View(list);
		}

		public ActionResult View(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			return View(item);
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

		public ActionResult DeleteAttachment(int id,string url)
		{
			url = url.FromBase64();

			WorkItemManager.DeleteAttachment(id,url);
			return RedirectToAction("Edit", new { id = id });
		}

		public ActionResult AllFields()
		{
			StringBuilder builder = new StringBuilder();

			WorkItemSummary summary = WorkItemManager.NewBug();
			IEnumerable<Field> list = summary.Fields.Cast<Field>().OrderBy(f => f.Name);
			foreach (Field field in list)
			{
				builder.AppendLine(string.Format("{0} [{1}] - {2}", field.Name, field.ReferenceName, string.Join(",", field.AllowedValues.Cast<string>())));
			}

			return Content(builder.ToString(),"text/plain");
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
				WorkItemManager.SaveBug(item); // item.Id is updated

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
				return RedirectToAction("New", new { id = item.Title});
			}
		}

		[HttpGet]
		public ActionResult Edit(int id,string fromUrl)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			item.IsNew = false;

			ViewData["fromUrl"] = fromUrl;
			ViewData["PageName"] = "Bug " + id;
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Severities"] = item.ValidSeverities;
			ViewData["Users"] = UserContext.Current.Users;

			return View(item);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(WorkItemSummary item,string fromUrl)
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

				if (string.IsNullOrEmpty(fromUrl))
					return RedirectToAction("View", new { id = item.Id });
				else
					return Redirect(fromUrl);
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;
				return RedirectToAction("Edit", new { id = item.Id});
			}
		}

		private string SaveFile(string fieldName,int id)
		{
			string filename = Request.Files[fieldName].FileName;
			if (!string.IsNullOrEmpty(filename))
			{
				string directory = string.Format(@"{0}{1}", SpruceSettings.UploadDirectory, id);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string filePath = string.Format(@"{0}\{1}",directory,filename);
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
			IEnumerable<WorkItemSummary> list = FilterAndPageList(GetBugFilterOptions(),"", true, "CreatedDate", true, 1, 10000, new BugManager());

			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				ViewData.Model = list;
				ViewEngineResult engineResult = ViewEngines.Engines.FindView(ControllerContext, "Excel", "_ExcelLayout");
				ViewContext context = new ViewContext(ControllerContext, engineResult.View, ViewData, TempData, writer);

				engineResult.View.Render(context, writer);
				writer.Close();

				FileContentResult result = new FileContentResult(Encoding.Default.GetBytes(builder.ToString()), "application/ms-excel");
				result.FileDownloadName = "bugs.xml";

				return result;
			}
		}

		public ActionResult Rss(string projectName, string areaPath,string iterationPath,string filter)
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

			IEnumerable<WorkItemSummary> list = FilterAndPageList(new FilterOptions(),projectName, true, "CreatedDate", true, 1, 10000, new BugManager());

			RssActionResult result = new RssActionResult();
			result.Feed = GetRssFeed(list,"Bugs");
			return result;
		}
    }
}
