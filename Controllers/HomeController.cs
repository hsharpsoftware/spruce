using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spruce.Models;

namespace Spruce
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
			WorkItemManager.Configure(User.Identity);

			return View(WorkItemManager.AllBugs());
        }

		public ActionResult AllItems()
		{
			return View(WorkItemManager.AllItems());
		}

		public ActionResult Active()
		{
			return View("Index", WorkItemManager.AllActiveBugs());
		}

		public ActionResult Closed()
		{
			return View("Index", WorkItemManager.AllClosedBugs());
		}

		public ActionResult Tasks()
		{
			return View("Index", WorkItemManager.AllTasks());
		}


		public ActionResult View(int id)
		{
			var x = WorkItemManager.ItemById(id);
			return View(x);
		}
    }
}
