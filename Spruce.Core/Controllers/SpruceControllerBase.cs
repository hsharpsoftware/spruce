using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using System.IO;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core.Controllers
{
	public class SpruceControllerBase<T> : Controller where T: WorkItemSummary,new()
	{
		public virtual ActionResult Index(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			return View();
		}

		public ActionResult View(int id)
		{
			T item = GetItem(id);
			return View(item);
		}

		public ActionResult Resolve(int id)
		{
			ResolveWorkItem(id);
			return RedirectToAction("View", new { id = id });
		}

		public ActionResult Close(int id)
		{
			CloseWorkItem(id);
			return RedirectToAction("Index");
		}

		public ActionResult DeleteAttachment(int id, string url)
		{
			url = url.FromBase64();
			DeleteWorkItemAttachment(id, url);

			return RedirectToAction("Edit", new { id = id });
		}

		protected virtual T GetItem(int id)
		{
			// We can get away with using the base class here, as no WorkItemType filter is used just an id.
			QueryManager manager = GetQueryManager();
			return manager.ItemById<T>(id);
		}

		protected virtual void ResolveWorkItem(int id)
		{
			GetManager().Resolve(id);
		}

		protected virtual void DeleteWorkItemAttachment(int id,string url)
		{
			GetManager().DeleteAttachment(id, url);
		}

		protected virtual void CloseWorkItem(int id)
		{
			GetManager().Close(id);
		}

		protected override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
			UserContext.Current.UpdateSettings();
		}

		protected SyndicationFeed GetRssFeed(IEnumerable<WorkItemSummary> list,string title)
		{
			SyndicationFeed feed = new SyndicationFeed();

			title = string.Format("{0} ({1} - {2})", title, UserContext.Current.CurrentProject.Name, ViewData["CurrentFilter"]);
			feed.Title = new TextSyndicationContent(title);

			List<SyndicationItem> items = new List<SyndicationItem>();
			foreach (WorkItemSummary summary in list)
			{
				SyndicationItem item = new SyndicationItem();
				item.Title = new TextSyndicationContent(string.Format("#{0} - {1}", summary.Id, summary.Title));
				item.PublishDate = summary.CreatedDate;
				item.Summary = new TextSyndicationContent(summary.Description);
				item.Content = new TextSyndicationContent(summary.Description);
				item.Authors.Add(new SyndicationPerson(summary.CreatedBy));

				string url = string.Format("{0}/{1}/view/{2}", SpruceSection.Current.SiteUrl, RouteData.Values["Controller"], summary.Id);
				item.AddPermalink(new Uri(url));

				items.Add(item);
			}

			feed.Items = items;
			return feed;
		}

		protected ListData<T> FilterAndPage(FilterOptions filterOptions, string projectName, bool isHeatMap, string sortBy, bool? descending, int? page, int? pageSize)
		{
			QueryManager manager = GetQueryManager();

			if (!string.IsNullOrEmpty(projectName))
			{
				if (projectName != UserContext.Current.CurrentProject.Name)
				{
					UserContext.Current.ChangeCurrentProject(projectName);
				}
			}

			if (!string.IsNullOrEmpty(filterOptions.Title))
			{
				manager.WithTitle(filterOptions.Title);
			}

			if (!string.IsNullOrEmpty(filterOptions.AssignedTo))
			{
				manager.AndAssignedTo(filterOptions.AssignedTo);
			}

			//
			// Status
			//
			if (filterOptions.Active)
			{
				manager.SetActive();
			}
			else if (filterOptions.Closed)
			{
				manager.SetClosed();
			}
			else if (filterOptions.Resolved)
			{
				manager.SetResolved();
			}

			//
			// Dates
			//
			if (filterOptions.StartDate > DateTime.MinValue)
			{
				manager.WithStartingFromDate(filterOptions.StartDate);
			}

			if (filterOptions.EndDate > DateTime.MinValue)
			{
				manager.WithEndingOnDate(filterOptions.EndDate);
			}

			IEnumerable<T> list = manager.Execute<T>();

			ListData<T> data = new ListData<T>();
			data.FilterValues.Title = filterOptions.Title;
			data.FilterValues.AssignedTo = filterOptions.AssignedTo;
			data.FilterValues.Status = filterOptions.ConvertStatusToString();
			data.WorkItems = list;

			if (filterOptions.StartDate > DateTime.MinValue)
				data.FilterValues.StartDate = filterOptions.StartDate.ToShortDateString();
			else
				data.FilterValues.StartDate = "";

			if (filterOptions.EndDate > DateTime.MinValue)
				data.FilterValues.EndDate = filterOptions.StartDate.ToShortDateString();
			else
				data.FilterValues.EndDate = "";
			
			
			PageList(data,isHeatMap,sortBy,descending,page,pageSize);

			return data;
		}

		/// <summary>
		/// Pages the list contained in the ListData.WorkItems
		/// </summary>
		protected void PageList(ListData<T> data, bool isHeatMap, string sortBy, bool? descending, int? page, int? pageSize)
		{
			//
			// Page the list
			//
			int currentPage = page.HasValue ? page.Value : 1;

			int pageSizeVal = UserContext.Current.Settings.PageSize;
			if (pageSizeVal == 0 || pageSize != pageSizeVal)
			{
				if (pageSize.HasValue)
					pageSizeVal = pageSize.Value;

				if (pageSizeVal < 10)
					pageSizeVal = 100;

				if (UserContext.Current.Settings.PageSize != pageSizeVal)
				{
					UserContext.Current.Settings.PageSize = pageSizeVal;
				}
			}

			Pager pager = new Pager(isHeatMap, sortBy, descending == true, pageSizeVal);
			data.WorkItems = pager.Page<T>(data.WorkItems, currentPage);

			data.FilterValues.PageCount = pager.PageCount;
			data.FilterValues.CurrentPage = pager.CurrentPageNumber;
			data.FilterValues.PageSize = pager.PageSize;
			data.FilterValues.IsDescending = (descending == true);
			data.FilterValues.SortBy = sortBy;
		}

		protected string SaveFile(string fieldName, int id)
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

		protected ActionResult Rss(IEnumerable<T> list, string title, string projectName, string areaPath, string iterationPath, string filter)
		{
			if (!string.IsNullOrEmpty(areaPath))
			{
				areaPath = HttpUtility.UrlDecode(areaPath);
				AreaSummary summary = UserContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == areaPath);
				UserContext.Current.Settings.AreaName = summary.Name;
				UserContext.Current.Settings.AreaPath = summary.Path;
			}
			else if (!string.IsNullOrEmpty(iterationPath))
			{
				areaPath = HttpUtility.UrlDecode(iterationPath);
				IterationSummary summary = UserContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iterationPath);
				UserContext.Current.Settings.IterationName = summary.Name;
				UserContext.Current.Settings.IterationPath = summary.Path;
			}

			RssActionResult result = new RssActionResult();
			result.Feed = GetRssFeed(list, title);
			return result;
		}

		protected ActionResult Excel(IEnumerable<T> list)
		{
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

		public ActionResult DumpFields()
		{
			// This should possibly be moved into a view
			T summary = GetManager().NewItem();
			IEnumerable<Field> list = summary.Fields.Cast<Field>().OrderBy(f => f.Name);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<html><style>body { font-family:Segoe UI, helvetica, } .border { font-size:8pt;border:solid #CCC 1px; margin:5px;padding:5px; } ");
			builder.AppendLine(".name { font-weight:bold; }");
			builder.AppendLine("</style><body>");

			builder.AppendFormat("<h1>{0}</h1>",summary.Fields[0].WorkItem.Type.Name);

			foreach (Field field in list)
			{
				builder.AppendLine("<div class=\"border\">");
				builder.AppendLine(string.Format("<div class=\"name\">{0}</div><div>[{1}]</div>",field.Name, field.ReferenceName));
				builder.AppendLine(string.Format("<div>IsRequired: {0}</div>", field.IsRequired));

				if (field.HasAllowedValuesList)
				{
					builder.AppendLine("<div>Allowed values: <ul>");

					foreach (string item in field.AllowedValues)
					{
						builder.AppendLine(string.Format("<li>{0}</li>", item));
					}
					builder.AppendLine("</ul></div>");
				}

				builder.AppendLine("</div>");
			}

			builder.AppendLine("</body></html>");

			return Content(builder.ToString(), "text/plain");
		}

		protected virtual WorkItemManager<T> GetManager()
		{
			throw new NotImplementedException("GetManager should be implemented by controllers inheriting from SpruceControllerBase");
		}

		protected virtual QueryManager GetQueryManager()
		{
			return new QueryManager();
		}

		protected void UpdateUserFilterOptions()
		{
			if (!string.IsNullOrEmpty(Request.QueryString["title"]) ||
				!string.IsNullOrEmpty(Request.QueryString["startDate"]) ||
				!string.IsNullOrEmpty(Request.QueryString["endDate"]) ||
				!string.IsNullOrEmpty(Request.QueryString["assignedTo"])
				)
			{
				UserContext.Current.Settings.UpdateBugFilterOptions(UserContext.Current.CurrentProject.Name,
					Request.QueryString["title"],
					Request.QueryString["startDate"],
					Request.QueryString["startDate"],
					Request.QueryString["endDate"],
					Request.QueryString["status"]);
			}
		}
	}
}
