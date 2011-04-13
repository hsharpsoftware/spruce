using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

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

		protected void SetHighlightedArea(string area)
		{		
			if (!string.IsNullOrEmpty(area))
			{
				AreaSummary summary = SpruceContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == area);
				SpruceContext.Current.UserSettings.AreaName = summary.Name;
				SpruceContext.Current.UserSettings.AreaPath = summary.Path;

				ViewData["CurrentAreaName"] = summary.Name;
				ViewData["CurrentAreaPath"] = summary.Path;
				ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
				SpruceContext.Current.UpdateUserSettings();
			}
		}

		protected void SetHighlightedIteration(string iteration)
		{
			if (!string.IsNullOrEmpty(iteration))
			{
				IterationSummary summary = SpruceContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iteration);
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

		protected void SetBugView(string actionName)
		{
			SpruceContext.Current.UserSettings.BugView = actionName;
			SpruceContext.Current.UpdateUserSettings();
		}

		protected void SetTaskView(string actionName)
		{
			SpruceContext.Current.UserSettings.TaskView = actionName;
			SpruceContext.Current.UpdateUserSettings();
		}
	}
}
