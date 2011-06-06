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
		public static string GetPreviousFieldValue(this HtmlHelper helper, WorkItemSummary model, string fieldName,int revisionNumber)
		{
			if (revisionNumber > 0 && model.Revisions[revisionNumber - 1].Fields[fieldName].Value != null)
				return model.Revisions[revisionNumber - 1].Fields[fieldName].Value.ToString();

			return "";
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
						newUrl += string.Format("{0}{1}={2}",hasQuery ? "&" : "", key, nameValues[key]);
						hasQuery = true;
					}
				}

				// Append our querystring item to the end
				newUrl += string.Format("{0}{1}={2}",hasQuery ? "&" : "",name,value);
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
				builder.Append("\"" +summary.Title+ "\"");
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

			// Priority
			builder.Append(summary.Priority);
			builder.Append(",");

			// Severity
			if (summary.Severity.IndexOf(",") > -1)
				builder.Append("\"" + summary.Severity + "\"");
			else
				builder.Append(summary.Title);


			return builder.AppendLine().ToString();
		}
	}
}