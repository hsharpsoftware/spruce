using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Spruce.Models
{
	public static class HelperExtensions
	{
		public static string DropDownBoxFromList(this HtmlHelper helper, string name, IList<string> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item, Value = item };
				if (item == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList).ToHtmlString();
		}

		public static string DropDownBoxForAreas(this HtmlHelper helper, string name, IList<AreaSummary> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (AreaSummary item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item.Name, Value = item.Path };
				if (item.Path == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList).ToHtmlString();
		}

		public static string DropDownBoxForIterations(this HtmlHelper helper, string name, IList<IterationSummary> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (IterationSummary item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item.Name, Value = item.Path };
				if (item.Path == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList).ToHtmlString();
		}
	}
}