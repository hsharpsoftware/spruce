using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;

namespace Spruce.Models
{
	public static class HelperExtensions
	{
		/// <summary>
		/// Attempts to parse the object as a string value and convert to an integer. If this fails, zero is returned.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int ToIntOrDefault(this object value)
		{
			if (value == null)
				return 0;

			int i = 0;
			if (int.TryParse(value.ToString(),out i))
				return i;
			else
				return 0;
		}

		public static string ToBase64(this string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
				return Convert.ToBase64String(Encoding.Default.GetBytes(value));
			else
				return "";
		}

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
					selectListItem.Selected = true;//

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

		public static string HtmlForListFilters(this HtmlHelper helper, string title,string description,string url)
		{
			string link = "<b>" +title+ "</b>";

			if ((string) HttpContext.Current.Session["ListLink"] != url)
				link = string.Format(@"<a href=""/Tfs/Spruce/Home/{0}"" title=""{1}"">{2}</a>",url,description,title);

			link += "&nbsp;|&nbsp;";

			return link;			
		}
	}
}