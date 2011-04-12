using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;
using Spruce.Core;

namespace Spruce.Core
{
	public static class HtmlExtensions
	{
		public static MvcHtmlString FormatAreaAndIterationName(this HtmlHelper helper, object iteration, object area)
		{
			string iterationName = iteration.ToString();
			string areaName = area.ToString();

			string result = "";
			if (!string.IsNullOrEmpty(iterationName) && iterationName != "None")
				result = iterationName;

			if (!string.IsNullOrEmpty(areaName) && areaName != "None")
			{
				if (!string.IsNullOrEmpty(result))
					result += "&nbsp;|&nbsp;";

				result += areaName;
			}

			return MvcHtmlString.Create(result);
		}

		public static MvcHtmlString DropDownBoxFromList(this HtmlHelper helper, string name, IList<string> items, string selectedValue, int tabIndex)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item, Value = item };
				if (item == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList, new { tabindex = tabIndex });
		}


		public static MvcHtmlString DropDownBoxForAreas(this HtmlHelper helper, string name, IList<AreaSummary> items, string selectedValue, int tabIndex)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (AreaSummary item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item.Name, Value = item.Path };
				if (item.Path == selectedValue)
					selectListItem.Selected = true;//

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList, new { tabindex = tabIndex });
		}

		public static MvcHtmlString DropDownBoxForIterations(this HtmlHelper helper, string name, IList<IterationSummary> items, string selectedValue, int tabIndex)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (IterationSummary item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item.Name, Value = item.Path };
				if (item.Path == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList, new { tabindex = tabIndex });
		}

		public static MvcHtmlString HtmlForListFilters(this HtmlHelper helper, string title, string description, string url)
		{
			string link = "<b>" + title + "</b>";

			if ((string)HttpContext.Current.Session["ListLink"] != url)
			{
				link = string.Format(@"<a href=""/Tfs/Spruce/Home/{0}"" title=""{1}"">{2}</a>", url, description, title);
			}

			link += "&nbsp;|&nbsp;";

			return new MvcHtmlString(link);
		}

		/// <summary>
		/// Limits the length of string by the provided maximum length, returning the string with "..." if it's longer.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static string Shorten(this string input, int maxLength)
		{
			if (input.Length > maxLength)
				return input.Substring(0, maxLength) + "...";
			else
				return input;
		}
	}
}