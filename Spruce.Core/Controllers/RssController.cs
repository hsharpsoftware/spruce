using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Spruce.Core.Controllers
{
	public class RssController : ControllerBase
    {
        //
        // GET: /Rss/

        public ActionResult Index()
        {
            return View();
        }

    }
}
