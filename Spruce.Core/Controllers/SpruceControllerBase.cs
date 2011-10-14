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
	/// <summary>
	/// The base controller for all templates in Spruce. This controller contains a set of helper actions for 
	/// common work item related views/actions.
	/// </summary>
	public class SpruceControllerBase: Controller
	{
		/// <summary>
		/// This serves as the root page for a work item, and contains the list of work items,
		/// filtered using the optional parameters provided.
		/// </summary>
		/// <param name="id">The project name (called 'id' so as the default routes can be used)</param>
		/// <param name="sortBy">The column to sort by</param>
		/// <param name="desc">Whether to sort by the column in descending or ascending (false) order.</param>
		/// <param name="page">The page number to show.</param>
		/// <param name="pageSize">The number of items per page.</param>
		/// <param name="title">A work item title to filter by.</param>
		/// <param name="assignedTo">The person the work item is assigned to.</param>
		/// <param name="startDate">Filters the start date of the work item.</param>
		/// <param name="endDate">Filters the end date of the work item.</param>
		/// <param name="status">Filters the status of the work item.</param>
		/// <remarks>This base method simply returns the view with no model.</remarks>
		public virtual ActionResult Index(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			return View();
		}

		/// <summary>
		/// Displays a work item's details using the provided id.
		/// </summary>
		public ActionResult View(int id)
		{
			return View(GetItem(id));
		}

		/// <summary>
		/// Sets a work item's state to 'resolved', and redirects to the view page. This action should handle <see cref="SaveExceptions"/> that the domain can throw.
		/// </summary>
		public ActionResult Resolve(int id)
		{
			ResolveWorkItem(id);
			return RedirectToAction("View", new { id = id });
		}

		/// <summary>
		/// Sets a work item's state to 'closed', and redirects to the view page. This action should handle <see cref="SaveExceptions"/> that the domain can throw.
		/// </summary>
		public ActionResult Close(int id)
		{
			CloseWorkItem(id);
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Attempts to delete an attachment with the provided url, for the work item with given id. The url parameter
		/// should be base64 encoded.
		/// </summary>
		/// <returns></returns>
		public ActionResult DeleteAttachment(int id, string url)
		{
			url = url.FromBase64();
			DeleteWorkItemAttachment(id, url);

			return RedirectToAction("Edit", new { id = id });
		}

		/// <summary>
		/// Gets a work item using its id as the lookup.
		/// </summary>
		protected virtual WorkItemSummary GetItem(int id)
		{
			// We can get away with using the base class here, as no WorkItemType filter is used just an id.
			QueryManager manager = new QueryManager();
			return manager.ItemById(id);
		}

		/// <summary>
		/// Uses the manager provided by the <see cref="GetManager()"/> method to resolve a work item.
		/// </summary>
		protected virtual void ResolveWorkItem(int id)
		{
			GetManager().Resolve(id);
		}

		/// <summary>
		/// Uses the manager provided by the <see cref="GetManager()"/> method to delete a work item attachment.
		/// </summary>
		protected virtual void DeleteWorkItemAttachment(int id,string url)
		{
			GetManager().DeleteAttachment(id, url);
		}

		/// <summary>
		/// Uses the manager provided by the <see cref="GetManager()"/> method to close a work item.
		/// </summary>
		protected virtual void CloseWorkItem(int id)
		{
			GetManager().Close(id);
		}


		/// <summary>
		/// Called after each action method is invoked, this overrides the default behaviour to 
		/// update the user settings.
		/// </summary>
		/// <param name="filterContext">Information about the current request and action.</param>
		protected override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
			UserContext.Current.Settings.Save(); // persist any project name, iteration changes
		}

		/// <summary>
		/// Creates an RSS syndication feed from the list of <see cref="WorkItemSummary"/> and sets the title (which also includes the project name and filter).
		/// </summary>
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

		/// <summary>
		/// Gets all work items of the type provided by T, and then filters, sorts and pages the list using the parameters provided.
		/// </summary>
		protected ListData FilterAndPage<T>(FilterOptions filterOptions, string projectName, string sortBy, bool? descending, int? page, int? pageSize)
			where T : WorkItemSummary, new()
		{
			QueryManager<T> manager = new QueryManager<T>();

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
				manager.WhereActive();
			}
			else if (filterOptions.Closed)
			{
				manager.WhereClosed();
			}
			else if (filterOptions.Resolved)
			{
				manager.WhereResolved();
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

			IEnumerable<WorkItemSummary> list = manager.Execute();

			ListData data = new ListData();
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
			
			
			PageList(data,sortBy,descending,page,pageSize);

			return data;
		}

		/// <summary>
		/// Pages the list contained in the provided <see cref="ListData.WorkItems"/>. This also updates the
		/// <see cref="ListData.FilterValues"/> properties with the parameter values.
		/// </summary>
		protected void PageList(ListData data, string sortBy, bool? descending, int? page, int? pageSize)
		{
			int currentPage = page.HasValue ? page.Value : 1;
			int pageSizeVal = UserContext.Current.Settings.PageSize;
			bool descendingVal = (descending == true);

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

			Pager pager = new Pager(sortBy, descendingVal, pageSizeVal);
			data.WorkItems = Page(pager, data.WorkItems, currentPage);

			data.FilterValues.PageCount = pager.PageCount;
			data.FilterValues.CurrentPage = pager.CurrentPageNumber;
			data.FilterValues.PageSize = pager.PageSize;
			data.FilterValues.IsDescending = !descendingVal;
			data.FilterValues.SortBy = sortBy;
		}

		/// <summary>
		/// By default this calls pager.Page with a <see cref="WorkItemSummary"/>, but this can be overridden by derived classes to use
		/// a specialized <see cref="WorkItemSummary"/> with the pager parameter.
		/// </summary>
		protected virtual IEnumerable<WorkItemSummary> Page(Pager pager,IEnumerable<WorkItemSummary> items, int currentPage)
		{
			return pager.Page<WorkItemSummary>(items, currentPage);
		}

		/// <summary>
		/// Saves the HTML field name (which should be a file field) to the work item with the provided id.
		/// </summary>
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

		/// <summary>
		/// Creates an RSS feed, returning an <see cref="RssActionResult"/> for the list of <see cref="WorkItemSummary"/> objects provided,
		/// which are filtered using the parameters given.
		/// </summary>
		protected ActionResult Rss(IEnumerable<WorkItemSummary> list, string title, string projectName, string areaPath, string iterationPath, string filter)
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

		/// <summary>
		/// Generates an Excel spreadsheet using the list of <see cref="WorkItemSummary"/> objects provided. This action uses 
		/// the Excel view, and dynamically generates XML using the default view engine and this view. The action returns a 
		/// file download of the Excel spreadsheet.
		/// </summary>
		protected ActionResult Excel(IEnumerable<WorkItemSummary> list)
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

		/// <summary>
		/// A helper action that creates a new (dummy) work item using the <see cref="GetManager()"/> method and displays 
		/// all the fields for this work item and the allowed values. This action returns plain text.
		/// </summary>
		/// <returns></returns>
		public ActionResult DumpFields()
		{
			// This should possibly be moved into a view
			WorkItemSummary summary = GetManager().NewItem();
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

		/// <summary>
		/// Retrieves a <see cref="WorkItemManager"/> for the work item type (a <see cref="WorkItemSummary"/> derived class) 
		/// that this controller is responsible for. By default, this method throws a <see cref="NotImplementedException"/>.
		/// </summary>
		protected virtual WorkItemManager GetManager()
		{
			throw new NotImplementedException("GetManager should be implemented by controllers inheriting from SpruceControllerBase");
		}

		/// <summary>
		/// Updates the <see cref="FilterOptions"/> for the user's currently selected project, for the view provided by the key parameter. 
		/// This checks and uses querystring values for the <see cref="FilterOptions"/> values.
		/// </summary>
		protected void UpdateUserFilterOptions(string key)
		{
			if (!string.IsNullOrEmpty(Request.QueryString["title"]) ||
				!string.IsNullOrEmpty(Request.QueryString["startDate"]) ||
				!string.IsNullOrEmpty(Request.QueryString["endDate"]) ||
				!string.IsNullOrEmpty(Request.QueryString["assignedTo"]) ||
				!string.IsNullOrEmpty(Request.QueryString["status"])
				)
			{
				FilterOptions filterOptions = FilterOptions.Parse(Request.QueryString["title"],
					Request.QueryString["assignedTo"],
					Request.QueryString["startDate"],
					Request.QueryString["endDate"],
					Request.QueryString["status"]);

				ProjectFilterOptions project = UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name);
				project.UpdateFilterOption(key, filterOptions);
			}
		}
	}
}
