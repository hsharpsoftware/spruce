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
	/// <summary>
	/// A collection of extension methods for the HtmlHelper and UrlHelper classes, for producing HTML.
	/// </summary>
	public static class HtmlExtensions
	{
		/// <summary>
		/// Parses the provided text and converts all Markdown syntax into HTML.
		/// </summary>
		public static MvcHtmlString ParseMarkdown(this HtmlHelper helper, string text)
		{
			Markdown markdown = new Markdown();
			return MvcHtmlString.Create(markdown.Transform(text));
		}

		/// <summary>
		/// Generates the HTML anchor link for a sorting link for the table of work items.
		/// </summary>
		/// <typeparam name="T">The WorkItemSummary type the link is for</typeparam>
		/// <param name="title">The title to display in the column.</param>
		/// <param name="column">The column name the link is for.</param>
		/// <param name="model">The model data for the page</param>
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

		/// <summary>
		/// Returns the previous value of the field for the revision number provided.
		/// </summary>
		public static string GetFieldValueForRevision(this HtmlHelper helper, WorkItemSummary model, string fieldName, int revisionNumber)
		{
			if (revisionNumber > 0 && model.Revisions[revisionNumber - 1].Fields[fieldName].Value != null)
				return model.Revisions[revisionNumber - 1].Fields[fieldName].Value.ToString();

			return "";
		}

		/// <summary>
		/// Generate an anchor link for the RSS page using the current project, area and iteration.
		/// </summary>
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

		/// <summary>
		/// Get the filename for the provided changeset path.
		/// </summary>
		public static MvcHtmlString ParseChangesetFile(this HtmlHelper helper, string value)
		{
			return MvcHtmlString.Create(Path.GetFileName(value));
		}

		/// <summary>
		/// Returns the relevant icon path for the provided change - add, delete, edit or unknown graphics.
		/// </summary>
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

		/// <summary>
		/// Creates a HTML drop down list from the provided <see cref="IEnumerable`string"/> of items, selects the value and 
		/// sets the tab order and name of the list.
		/// </summary>
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
		/// Creates a HTML drop down list from the provided <see cref="IDictionary`string"/> of items, selects the value and name of the list.
		/// </summary>
		public static MvcHtmlString DropDownList(this HtmlHelper helper, string name, IDictionary<string,string> items,string selectedValue)
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

		/// <summary>
		/// Generates the drop down list of project names.
		/// </summary>
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

		/// <summary>
		/// Generates a drop down list for the number of items on a page.
		/// </summary>
		public static MvcHtmlString DropDownBoxForPageSize(this HtmlHelper helper)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			selectList.Add(new SelectListItem() { Text = "25", Value = "25",Selected=UserContext.Current.Settings.PageSize == 25 });
			selectList.Add(new SelectListItem() { Text = "50", Value = "50", Selected = UserContext.Current.Settings.PageSize == 50 });
			selectList.Add(new SelectListItem() { Text = "100", Value = "100", Selected = UserContext.Current.Settings.PageSize == 100 });

			return helper.DropDownList("pageSize", selectList);
		}

		/// <summary>
		/// Generates the drop down list of area names for the current project and selects the current area. The value of each item is the area path.
		/// </summary>
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

		/// <summary>
		/// Generates the drop down list of iteration names for the current project and selects the current iteration. The value of each item is the iteration path.
		/// </summary>
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

		/// <summary>
		/// Generates the drop down list of stored queries for the current project.
		/// </summary>
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
		public static string Shorten(this string input, int maxLength)
		{
			if (input.Length > maxLength)
				return input.Substring(0, maxLength) + "...";
			else
				return input;
		}

		/// <summary>
		/// Attempts to append a value to the querystring (used primarily for paging).
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

		/// <summary>
		/// Renders the header for the current template, by searching for a NavigationHeader.cshtml file and rendering this as HTML using
		/// the current ViewEngine.
		/// </summary>
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
