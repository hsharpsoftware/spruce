using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Raven.Storage.Esent;

namespace Spruce.Core
{
	/// <summary>
	/// The default entry point (the global.asax) for the web appliction.
	/// </summary>
	public class SpruceApplication : HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{*template}", new { template = @"(.*/)?template.css(/.*)?" });
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new {
					controller = SpruceSection.Current.DefaultController, 
					action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		protected void Application_Start()
		{
			// Register the new view engine which adds extra view paths to search
			ViewEngines.Engines.Clear();
			ExtendedRazorViewEngine engine = new ExtendedRazorViewEngine();
			engine.AddViewLocationFormat("~/Template/Views/{1}/{0}.cshtml");
			engine.AddViewLocationFormat("~/Template/Views/{1}/{0}.vbhtml");
			engine.AddPartialViewLocationFormat("~/Template/Views/{0}.cshtml");
			engine.AddPartialViewLocationFormat("~/Template/Views/{0}.vbhtml");
			ViewEngines.Engines.Add(engine);

			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}
	}
}
