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

			viewData["CurrentIterationName"] = SpruceContext.Current.UserSettings.IterationName;
			viewData["CurrentIterationPath"] = SpruceContext.Current.UserSettings.IterationPath;
			viewData["CurrentAreaName"] = SpruceContext.Current.UserSettings.AreaName;
			viewData["CurrentAreaPath"] = SpruceContext.Current.UserSettings.AreaPath;

			viewData["Projects"] = SpruceContext.Current.ProjectNames;
			viewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			viewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
		}
	}
}
