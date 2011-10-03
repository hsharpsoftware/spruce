using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;
using Spruce.Core;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Spruce.Core
{
	public static class HtmlExtensions
	{
		public static MvcHtmlString ParseMarkdown(this HtmlHelper helper, string text)
		{
			text = text.Replace("\n","<br/>");
			Markdown markdown = new Markdown();
			return MvcHtmlString.Create(markdown.Transform(text));
		}

		public static MvcHtmlString TableSortLink<T>(this HtmlHelper helper, string title, string column, ListData model)
			where T: WorkItemSummary
		{

			// Re-assemble the current querystring, with sortBy and desc removed.
			NameValueCollection queryCollection = HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.Query);

			string url = HttpContext.Current.Request.Url.LocalPath;

			if (queryCollection["sortBy"] != null)
				queryCollection.Remove("sortBy");
			
			if (queryCollection["desc"] != null)
				queryCollection.Remove("desc");

			url += "?";
			if (queryCollection.Count > 0)
			{
				// Url encoding for key/values?
				List<string> keysAndValues = new List<string>();
				foreach (string key in queryCollection)
				{
					keysAndValues.Add(string.Format("{0}={1}", key, queryCollection[key]));
				}

				url += string.Join("&",keysAndValues);
				url += "&";
			}

			bool descending = model.FilterValues.IsDescending;
			return MvcHtmlString.Create(string.Format(@"<a href=""{0}sortBy={1}&desc={2}"">{3}</a>", url, column, descending, title));
		}

		public static string GetFieldValueForRevision(this HtmlHelper helper, WorkItemSummary model, string fieldName, int revisionNumber)
		{
			if (revisionNumber > 0 && model.Revisions[revisionNumber - 1].Fields[fieldName].Value != null)
				return model.Revisions[revisionNumber - 1].Fields[fieldName].Value.ToString();

			return "";
		}

		public static string RssLink(this UrlHelper helper)
		{
			return helper.Action("Rss", new
			{
				projectName = helper.Encode(UserContext.Current.CurrentProject.Name),
				areaPath = helper.Encode(UserContext.Current.Settings.AreaPath),
				iteration = helper.Encode(UserContext.Current.Settings.IterationPath),
				filter = ""
			});
		}

		public static MvcHtmlString ParseChangesetFile(this HtmlHelper helper, string value)
		{
			return MvcHtmlString.Create(Path.GetFileName(value));
		}

		public static MvcHtmlString ChangesetIcon(this UrlHelper helper, Change change)
		{
			string icon = "";

			if ((change.ChangeType & ChangeType.Add) == ChangeType.Add)
			{
				icon = helper.Content("~/Assets/Images/changeset_add.png");
			}
			else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
			{
				icon = helper.Content("~/Assets/Images/changeset_delete.png");
			}
			else if ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit)
			{
				icon = helper.Content("~/Assets/Images/changeset_edit.png");
			}
			else
			{
				icon = helper.Content("~/Assets/Images/changeset_unknown.png");
			}

			return MvcHtmlString.Create(string.Format("<img src=\"{0}\" alt=\"{1}\" border=\"0\">", icon, change.ChangeType));
		}

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

		public static MvcHtmlString DropDownBoxFromList(this HtmlHelper helper, string name, IEnumerable<string> items, string selectedValue, int tabIndex)
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

		/// <summary>
		/// A shortcut for a dropdown list
		/// </summary>
		/// <param name="name">the select object's name</param>
		/// <param name="items">Where key is the value, and the value is the text</param>
		/// <param name="selectedValue"></param>
		/// <returns></returns>
		public static MvcHtmlString DropDownList(this HtmlHelper helper, string name, Dictionary<string,string> items,string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string key in items.Keys)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = items[key], Value = key };
				if (key == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList);
		}

		public static MvcHtmlString DropDownBoxForProjects(this HtmlHelper helper, int tabIndex)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			selectList.Add(new SelectListItem() { Text = "Select a project...", Value = "" });

			foreach (string project in UserContext.Current.ProjectNames)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = project, Value = project };
				if (project == UserContext.Current.CurrentProject.Name)
					selectListItem.Selected = true;//

				selectList.Add(selectListItem);
			}

			return helper.DropDownList("project", selectList, new { tabindex = tabIndex });
		}

		public static MvcHtmlString DropDownBoxForPageSize(this HtmlHelper helper)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			selectList.Add(new SelectListItem() { Text = "25", Value = "25",Selected=UserContext.Current.Settings.PageSize == 25 });
			selectList.Add(new SelectListItem() { Text = "50", Value = "50", Selected = UserContext.Current.Settings.PageSize == 50 });
			selectList.Add(new SelectListItem() { Text = "100", Value = "100", Selected = UserContext.Current.Settings.PageSize == 100 });

			return helper.DropDownList("pageSize", selectList);
		}

		public static MvcHtmlString DropDownBoxForAreas(this HtmlHelper helper, string name, IList<AreaSummary> items, string selectedValue, int tabIndex)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			selectList.Add(new SelectListItem() { Text = "Select an area...", Value = "" });

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

			selectList.Add(new SelectListItem() { Text = "Select an iteration...", Value = "" });

			foreach (IterationSummary item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item.Name, Value = item.Path };
				if (item.Path == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList, new { tabindex = tabIndex });
		}

		public static MvcHtmlString DropDownBoxForStoredQueries(this HtmlHelper helper, string name, IList<StoredQuerySummary> items, Guid selectedValue, int tabIndex)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (StoredQuerySummary item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item.Name, Value = item.Id.ToString() };
				if (item.Id == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList, new { tabindex = tabIndex });
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

		/// <summary>
		/// Attempts to add 'page=1' to the querystring of a url.
		/// </summary>
		public static string AppendQueryString(this Uri url, string name, object value)
		{
			string newUrl = url.ToString();

			if (!string.IsNullOrEmpty(url.Query))
				newUrl = newUrl.Replace(url.Query, "");

			var nameValues = HttpUtility.ParseQueryString(url.Query);

			if (nameValues.Keys.Count > 0)
			{
				newUrl += "?";
				bool hasQuery = false;

				// Append all querystring name/values except the 'name' one.
				foreach (string key in nameValues.Keys)
				{
					if (!string.IsNullOrEmpty(key) && key != name)
					{
						newUrl += string.Format("{0}{1}={2}", hasQuery ? "&" : "", key, nameValues[key]);
						hasQuery = true;
					}
				}

				// Append our querystring item to the end
				newUrl += string.Format("{0}{1}={2}", hasQuery ? "&" : "", name, value);
			}
			else
			{
				newUrl += string.Format("?{0}={1}", name, value);
			}

			return newUrl;
		}

		public static string ToCsv(this WorkItemSummary summary)
		{
			StringBuilder builder = new StringBuilder();

			// Title
			if (summary.Title.IndexOf(",") > -1)
				builder.Append("\"" + summary.Title + "\"");
			else
				builder.Append(summary.Title);

			builder.Append(",");

			// ID
			builder.Append(summary.Id);
			builder.Append(",");

			// Assigned to
			if (summary.AssignedTo.IndexOf(",") > -1)
				builder.Append("\"" + summary.AssignedTo + "\"");
			else
				builder.Append(summary.AssignedTo);

			builder.Append(",");

			// Created on
			builder.Append(summary.CreatedDate.ToString("ddd dd MMM yyyy"));
			builder.Append(",");

			// State
			if (summary.State.IndexOf(",") > -1)
				builder.Append("\"" + summary.State + "\"");
			else
				builder.Append(summary.State);

			builder.Append(",");

			return builder.AppendLine().ToString();
		}

		public static MvcHtmlString RenderHeader(this HtmlHelper helper)
		{
			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				ControllerContext controllerContext = helper.ViewContext.Controller.ControllerContext;
				ViewEngineResult engineResult = ViewEngines.Engines.FindPartialView(controllerContext, "NavigationHeader");
				ViewContext context = new ViewContext(controllerContext, engineResult.View, helper.ViewData, helper.ViewContext.TempData, writer);

				engineResult.View.Render(context, writer);
				writer.Close();

				return MvcHtmlString.Create(builder.ToString());
			}
		}
	}
}
