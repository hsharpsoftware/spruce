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
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

		protected void SetProject(string project)
		{
			if (!string.IsNullOrEmpty(project))
			{
				if (project != UserContext.Current.CurrentProject.Name)
				{
					UserContext.Current.ChangeCurrentProject(project);
					UserContext.Current.UpdateSettings();
				}
			}	
		}

		protected void SetArea(string areaPath)
		{		
			if (!string.IsNullOrEmpty(areaPath))
			{
				AreaSummary summary = UserContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == areaPath);
				UserContext.Current.Settings.AreaName = summary.Name;
				UserContext.Current.Settings.AreaPath = summary.Path;
				UserContext.Current.UpdateSettings();
			}
		}

		protected void SetIteration(string iterationPath)
		{
			if (!string.IsNullOrEmpty(iterationPath))
			{
				IterationSummary summary = UserContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iterationPath);
				UserContext.Current.Settings.IterationName = summary.Name;
				UserContext.Current.Settings.IterationPath = summary.Path;
				UserContext.Current.UpdateSettings();
			}
		}

		/// <summary>
		/// Sets the user settings for either heatmap or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		protected void SetBugView(string actionName)
		{
			UserContext.Current.Settings.BugView = actionName;
			UserContext.Current.UpdateSettings();
		}

		/// <summary>
		/// Sets the user settings for either a post it note view or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		protected void SetTaskView(string actionName)
		{
			UserContext.Current.Settings.TaskView = actionName;
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

		protected IEnumerable<WorkItemSummary> FilterAndPageList(string projectName, bool isHeatMap, string sortBy, bool? descending, int? page, int? pageSize, WorkItemManager manager)
		{
			if (!string.IsNullOrEmpty(projectName))
				SetProject(projectName);

			if (UserContext.Current.Settings.FilterOptions.Active)
			{
				manager.Active();
			}

			if (UserContext.Current.Settings.FilterOptions.AssignedToMe)
			{
				manager.AssignedToMe();
			}

			if (UserContext.Current.Settings.FilterOptions.Closed)
			{
				manager.Closed();
			}

			if (UserContext.Current.Settings.FilterOptions.Resolved)
			{
				manager.Resolved();
			}

			if (UserContext.Current.Settings.FilterOptions.ThisMonth)
			{
				manager.ThisMonth();
			}

			if (UserContext.Current.Settings.FilterOptions.ThisWeek)
			{
				manager.ThisWeek();
			}

			if (UserContext.Current.Settings.FilterOptions.Today)
			{
				manager.Today();
			}

			if (UserContext.Current.Settings.FilterOptions.LastMonth)
			{
				manager.LastMonth();
			}

			IEnumerable<WorkItemSummary> list = manager.ExecuteQuery();

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

				UserContext.Current.Settings.PageSize = pageSizeVal;
				UserContext.Current.UpdateSettings();
			}

			Pager pager = new Pager(isHeatMap, sortBy, descending == true, pageSizeVal);
			list = pager.Page<WorkItemSummary>(list, currentPage);

			ViewData["pageCount"] = pager.PageCount;
			ViewData["currentPage"] = currentPage;
			ViewData["pageSize"] = pageSizeVal;
			ViewData["desc"] = (descending == true);

			return list;
		}
	}
}
