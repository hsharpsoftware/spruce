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
			ViewData["CurrentUser"] = SpruceContext.Current.CurrentUser;
			ViewData["CurrentProjectName"] = SpruceContext.Current.CurrentProject.Name;

			ViewData["CurrentIterationName"] = SpruceContext.Current.UserSettings.IterationName;
			ViewData["CurrentIterationPath"] = SpruceContext.Current.UserSettings.IterationPath;
			ViewData["CurrentAreaName"] = SpruceContext.Current.UserSettings.AreaName;
			ViewData["CurrentAreaPath"] = SpruceContext.Current.UserSettings.AreaPath;
			ViewData["CurrentFilter"] = SpruceContext.Current.UserSettings.FilterType.GetDescription();
			ViewData["CurrentBugView"] = SpruceContext.Current.UserSettings.BugView;
			ViewData["CurrentTaskView"] = SpruceContext.Current.UserSettings.TaskView;

			ViewData["Projects"] = SpruceContext.Current.ProjectNames;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Filters"] = SpruceContext.Current.FilterTypes;	
		}

		protected void SetHighlightedProject(string project)
		{
			if (!string.IsNullOrEmpty(project))
			{
				if (project != SpruceContext.Current.CurrentProject.Name)
				{
					SpruceContext.Current.SetCurrentProject(project);
					ViewData["CurrentProjectName"] = project;
				}
			}	
		}

		protected void SetHighlightedArea(string areaPath)
		{		
			if (!string.IsNullOrEmpty(areaPath))
			{
				AreaSummary summary = SpruceContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == areaPath);
				SpruceContext.Current.UserSettings.AreaName = summary.Name;
				SpruceContext.Current.UserSettings.AreaPath = summary.Path;

				ViewData["CurrentAreaName"] = summary.Name;
				ViewData["CurrentAreaPath"] = summary.Path;
				ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
				SpruceContext.Current.UpdateUserSettings();
			}
		}

		protected void SetHighlightedIteration(string iterationPath)
		{
			if (!string.IsNullOrEmpty(iterationPath))
			{
				IterationSummary summary = SpruceContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iterationPath);
				SpruceContext.Current.UserSettings.IterationName = summary.Name;
				SpruceContext.Current.UserSettings.IterationPath = summary.Path;

				ViewData["CurrentIterationName"] = summary.Name;
				ViewData["CurrentIterationPath"] = summary.Path;
				ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
				SpruceContext.Current.UpdateUserSettings();
			}
		}

		protected void SetHighlightedFilter(string filter)
		{
			if (!string.IsNullOrEmpty(filter))
			{
				FilterType filterType;
				if (Enum.TryParse<FilterType>(filter, out filterType))
				{
					SpruceContext.Current.UserSettings.FilterType = filterType;	
					ViewData["CurrentFilter"] = filterType.ToString();
					SpruceContext.Current.UpdateUserSettings();
				}
			}
		}

		/// <summary>
		/// Sets the user settings for either heatmap or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		protected void SetBugView(string actionName)
		{
			SpruceContext.Current.UserSettings.BugView = actionName;
			SpruceContext.Current.UpdateUserSettings();
		}

		/// <summary>
		/// Sets the user settings for either a post it note view or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		protected void SetTaskView(string actionName)
		{
			SpruceContext.Current.UserSettings.TaskView = actionName;
			SpruceContext.Current.UpdateUserSettings();
		}

		protected SyndicationFeed GetRssFeed(IEnumerable<WorkItemSummary> list,string controller)
		{
			SyndicationFeed feed = new SyndicationFeed();

			string title = string.Format("Bugs ({0} - {1})", ViewData["CurrentProjectName"], ViewData["CurrentFilter"]);
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
	}
}
