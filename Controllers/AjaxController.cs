using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spruce.Models;

namespace Spruce.Controllers
{
    public class AjaxController : Controller
    {
		public ActionResult GetIterationsForProject(string id)
		{
			IList<IterationSummary> iterations = WorkItemManager.IterationsForProject(id);

			List<JsonDetail> json = new List<JsonDetail>();
			if (iterations.Count == 0)
			{
				json.Add(new JsonDetail(id, id));
			}
			else
			{
				if (iterations.Count > 1)
					json.Add(new JsonDetail("", "Any"));

				foreach (IterationSummary iteration in iterations)
				{
					json.Add(new JsonDetail(iteration.Path, iteration.Name));
				}
			}

			return Json(json,JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetAreasForProject(string id)
		{
			IList<AreaSummary> areas = WorkItemManager.AreasForProject(id);

			List<JsonDetail> json = new List<JsonDetail>();
			if (areas.Count == 0)
			{
				json.Add(new JsonDetail(id, id));
			}
			else
			{
				if (areas.Count > 1)
					json.Add(new JsonDetail("", "Any"));

				foreach (AreaSummary area in areas)
				{
					json.Add(new JsonDetail(area.Path,area.Name));
				}
			}

			return Json(json, JsonRequestBehavior.AllowGet);
		}

		private class JsonDetail
		{
			public string id { get; set; }
			public string label { get; set; }

			public JsonDetail(string id, string label)
			{
				this.id = id;
				this.label = label;
			}
		}
    }
}
