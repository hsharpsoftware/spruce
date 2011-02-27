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

			ViewData["CurrentIterationName"] = SpruceContext.Current.FilterSettings.IterationName;
			ViewData["CurrentIterationPath"] = SpruceContext.Current.FilterSettings.IterationPath;
			ViewData["CurrentAreaName"] = SpruceContext.Current.FilterSettings.AreaName;
			ViewData["CurrentAreaPath"] = SpruceContext.Current.FilterSettings.AreaPath;

			ViewData["Projects"] = SpruceContext.Current.ProjectNames;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
		}

		protected void SetHighlightedProject(string project)
		{
			if (!string.IsNullOrEmpty(project))
			{
				if (project != SpruceContext.Current.CurrentProject.Name)
				{
					SpruceContext.Current.SetProject(project);
					ViewData["CurrentProjectName"] = project;
				}
			}	
		}

		protected void SetHighlightedArea(string area)
		{		
			if (!string.IsNullOrEmpty(area))
			{
				AreaSummary summary = SpruceContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == area);
				SpruceContext.Current.FilterSettings.AreaName = summary.Name;
				SpruceContext.Current.FilterSettings.AreaPath = summary.Path;

				ViewData["CurrentAreaName"] = summary.Name;
				ViewData["CurrentAreaPath"] = summary.Path;
				ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			}
		}

		protected void SetHighlightedIteration(string iteration)
		{
			if (!string.IsNullOrEmpty(iteration))
			{
				IterationSummary summary = SpruceContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iteration);
				SpruceContext.Current.FilterSettings.IterationName = summary.Name;
				SpruceContext.Current.FilterSettings.IterationPath = summary.Path;

				ViewData["CurrentIterationName"] = summary.Name;
				ViewData["CurrentIterationPath"] = summary.Path;
				ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			}
		}
	}
}
