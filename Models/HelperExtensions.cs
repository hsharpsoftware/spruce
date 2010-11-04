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
		public static string DropDownBoxForMaximumItems(this HtmlHelper helper, string name)
		{
			int selectedValue = SpruceContext.Current.FilterSettings.MaximumItems;
			List<SelectListItem> selectList = new List<SelectListItem>();

			// There's only 4 options so it's easier to hard code them
			SelectListItem selectListItem = new SelectListItem() { Text = "All", Value = "0" };
			if (selectedValue == 0)
				selectListItem.Selected = true;
			selectList.Add(selectListItem);

			selectListItem = new SelectListItem() { Text = "25", Value = "25" };
			if (selectedValue == 25)
				selectListItem.Selected = true;
			selectList.Add(selectListItem);

			selectListItem = new SelectListItem() { Text = "50", Value = "50" };
			if (selectedValue == 50)
				selectListItem.Selected = true;
			selectList.Add(selectListItem);

			selectListItem = new SelectListItem() { Text = "100", Value = "100" };
			if (selectedValue == 100)
				selectListItem.Selected = true;
			selectList.Add(selectListItem);

			return helper.DropDownList(name, selectList).ToHtmlString();
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

		public static string DropDownBoxForProjects(this HtmlHelper helper, string name)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();
			IList<string> items = SpruceContext.Current.ProjectNames;
			string selectedValue = SpruceContext.Current.CurrentProject.Name;//

			foreach (string item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item, Value = item };
				if (item == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList).ToHtmlString();
		}

		public static string DropDownBoxForAreas(this HtmlHelper helper, string name)
		{
			string selectedValue = SpruceContext.Current.FilterSettings.AreaPath;
			List<SelectListItem> selectList = new List<SelectListItem>();
			IList<AreaSummary> items = SpruceContext.Current.CurrentProject.Areas.ToList();

			// Only one area usually indicates there are none (and TFS defaults to the project name)
			if (items.Count > 1)
				items.Insert(0, new AreaSummary() { Name = "Any", Path = "" });		

			return DropDownBoxForAreas(helper, name, items, selectedValue);
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

		public static string DropDownBoxForIterations(this HtmlHelper helper, string name)
		{
			string selectedValue = SpruceContext.Current.FilterSettings.IterationPath;
			List<SelectListItem> selectList = new List<SelectListItem>();
			IList<IterationSummary> items = SpruceContext.Current.CurrentProject.Iterations.ToList();

			// Only one iteration usually indicates there are none (and TFS defaults to the project name)
			if (items.Count > 1)
				items.Insert(0, new IterationSummary() { Name = "Any", Path = "" });		

			return DropDownBoxForIterations(helper, name, items, selectedValue);
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