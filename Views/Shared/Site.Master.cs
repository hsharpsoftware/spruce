using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Spruce.Models;

namespace Spruce
{
	public partial class Site : ViewMasterPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ViewData["CurrentUser"] = SpruceContext.Current.CurrentUser;
			ViewData["CurrentProjectName"] = SpruceContext.Current.CurrentProject.Name;

			bool hasIterationData = !string.IsNullOrWhiteSpace(SpruceContext.Current.FilterSettings.IterationPath);
			bool hasAreaData = !string.IsNullOrWhiteSpace(SpruceContext.Current.FilterSettings.AreaPath);
			ViewData["HasIterationData"] = hasIterationData;
			ViewData["HasAreaData"] = hasAreaData;

			if (hasIterationData)
			{
				ViewData["IterationPath"] = SpruceContext.Current.FilterSettings.IterationPath.Replace(SpruceContext.Current.CurrentProject.Name + "\\", "");
			}

			if (hasAreaData)
			{
				ViewData["AreaPath"] = SpruceContext.Current.FilterSettings.AreaPath.Replace(SpruceContext.Current.CurrentProject.Name + "\\", "");
			}
		}
	}
}