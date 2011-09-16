using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ServiceModel.Syndication;

namespace Spruce.Core.Controllers
{
	public class ControllerBase : Controller
	{
		/// <summary>
		/// Sets the user settings for either heatmap or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		protected void SetBugView(string actionName)
		{
			UserContext.Current.Settings.BugView = actionName;
		}

		protected override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
			UserContext.Current.UpdateSettings();
		}

		protected SyndicationFeed GetRssFeed(IEnumerable<WorkItemSummary> list,string controller)
		{
			SyndicationFeed feed = new SyndicationFeed();

			string title = string.Format("Bugs ({0} - {1})", UserContext.Current.CurrentProject.Name, ViewData["CurrentFilter"]);
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

				string url = string.Format("{0}/{1}/view/{2}",SpruceSection.Current.SiteUrl,controller,summary.Id);
				item.AddPermalink(new Uri(url));

				items.Add(item);
			}

			feed.Items = items;
			return feed;
		}

		protected IEnumerable<WorkItemSummary> FilterAndPageList(FilterOptions filterOptions, string projectName, bool isHeatMap, string sortBy, bool? descending, int? page, int? pageSize, WorkItemManager manager)
		{
			if (!string.IsNullOrEmpty(projectName))
			{
				if (projectName != UserContext.Current.CurrentProject.Name)
				{
					UserContext.Current.ChangeCurrentProject(projectName);
				}
			}

			if (!string.IsNullOrEmpty(filterOptions.Title))
			{
				manager.Title(filterOptions.Title);
			}

			if (!string.IsNullOrEmpty(filterOptions.AssignedTo))
			{
				manager.AssignedTo(filterOptions.AssignedTo);
			}

			//
			// Status
			//
			if (filterOptions.Active)
			{
				manager.Active();
			}
			else if (filterOptions.Closed)
			{
				manager.Closed();
			}
			else if (filterOptions.Resolved)
			{
				manager.Resolved();
			}

			//
			// Dates
			//
			if (filterOptions.StartDate > DateTime.MinValue)
			{
				manager.StartingFromDate(filterOptions.StartDate);
			}

			if (filterOptions.EndDate > DateTime.MinValue)
			{
				manager.EndingOnDate(filterOptions.EndDate);
			}

			IEnumerable<WorkItemSummary> list = manager.ExecuteQuery();

			ViewData["title"] = filterOptions.Title;
			ViewData["assignedTo"] = filterOptions.AssignedTo;
			ViewData["status"] = filterOptions.ConvertStatusToString();

			if (filterOptions.StartDate > DateTime.MinValue)
				ViewData["startDate"] = filterOptions.StartDate;
			else
				ViewData["startDate"] = "";

			if (filterOptions.EndDate > DateTime.MinValue)
				ViewData["endDate"] = filterOptions.StartDate;
			else
				ViewData["endDate"] = "";
			

			return PageList(list,isHeatMap,sortBy,descending,page,pageSize);
		}

		protected IEnumerable<WorkItemSummary> PageList(IEnumerable<WorkItemSummary> list,bool isHeatMap, string sortBy, bool? descending, int? page, int? pageSize)
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
			list = pager.Page<WorkItemSummary>(list, currentPage);

			ViewData["pageCount"] = pager.PageCount;
			ViewData["currentPage"] = currentPage;
			ViewData["pageSize"] = pageSizeVal;
			ViewData["desc"] = (descending == true);

			return list;
		}

		protected FilterOptions GetBugFilterOptions()
		{
			return  UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name).BugFilterOptions;
		}

		protected FilterOptions GetBugHeatmapFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name).BugHeatmapFilterOptions;
		}

		protected FilterOptions GetTaskFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name).TaskFilterOptions;
		}

		protected bool QueryStringContainsFilters()
		{
			if (!string.IsNullOrEmpty(Request.QueryString["title"]) ||
				!string.IsNullOrEmpty(Request.QueryString["startDate"]) ||
				!string.IsNullOrEmpty(Request.QueryString["endDate"]) ||
				!string.IsNullOrEmpty(Request.QueryString["assignedTo"])
				)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
