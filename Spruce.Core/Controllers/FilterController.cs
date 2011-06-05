using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spruce.Core.Controllers
{
	public class FilterController : ControllerBase
    {
		public ActionResult Index(string fromUrl,string areaPath,string iterationPath,FilterOptions filter)
		{
			UserContext.Current.Settings.AreaPath = areaPath.FromBase64();
			UserContext.Current.Settings.IterationPath = iterationPath.FromBase64();
			UserContext.Current.Settings.FilterOptions = filter;
			UserContext.Current.UpdateSettings();

			return Redirect(fromUrl);
		}
    }
}
