using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Spruce.Core
{
	public class SpruceApplication : HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
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
			engine.AddViewLocationFormat("~/Template/{1}/{0}.cshtml");
			engine.AddViewLocationFormat("~/Template/{1}/{0}.vbhtml");
			ViewEngines.Engines.Add(engine);

			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}
	}
}
