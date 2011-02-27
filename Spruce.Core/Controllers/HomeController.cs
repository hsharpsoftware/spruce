using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;

namespace Spruce.Core.Controllers
{
	public class HomeController : ControllerBase
    {
		public ActionResult Index()
		{
			// Dashboard
			return View("Index", WorkItemManager.AllBugs().ToList());
		}

		public ActionResult SetProject(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetHighlightedProject(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetIteration(string id,string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetHighlightedIteration(id);

			return Redirect(fromUrl);
		}

		public ActionResult SetArea(string id, string fromUrl)
		{
			TempData["RedirectedFromHomeController"] = true;
			id = Encoding.Default.GetString(Convert.FromBase64String(id));
			SetHighlightedArea(id);

			return Redirect(fromUrl);
		}		
    }
}
