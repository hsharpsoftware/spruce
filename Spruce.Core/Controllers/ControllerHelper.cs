using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spruce.Core.Controllers
{
	public class ControllerHelper
	{
		public static void SetViewData(ViewDataDictionary viewData)
		{
			viewData["CurrentUser"] = SpruceContext.Current.CurrentUser;
			viewData["CurrentProjectName"] = SpruceContext.Current.CurrentProject.Name;

			viewData["CurrentIterationName"] = SpruceContext.Current.FilterSettings.IterationName;
			viewData["CurrentIterationPath"] = SpruceContext.Current.FilterSettings.IterationPath;
			viewData["CurrentAreaName"] = SpruceContext.Current.FilterSettings.AreaName;
			viewData["CurrentAreaPath"] = SpruceContext.Current.FilterSettings.AreaPath;

			viewData["Projects"] = SpruceContext.Current.ProjectNames;
			viewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			viewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
		}
	}
}
