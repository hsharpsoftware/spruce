using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
    /// <summary>
    /// Extension methods for the Spruce domain and core.
    /// </summary>
    public static class CoreExtensions
	{
		/// <summary>
		/// Reports the index of the first occurrence of the specified project name in the list of <c>ProjectFilterOptions</c>
		/// </summary>
		public static int IndexOf(this IList<ProjectFilterOptions> options, string projectName)
		{
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i].Name == projectName)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Attempts to parse the string value and convert to an integer. If this fails, zero is returned.
		/// </summary>
		public static int ToIntOrDefault(this string value)
		{
			if (value == null)
				return 0;

			int i = 0;
			if (int.TryParse(value.ToString(),out i))
				return i;
			else
				return 0;
		}

		/// <summary>
		/// Attempts to parse the string value and convert to a double. If this fails, zero is returned.
		/// </summary>
		public static double ToDoubleOrDefault(this string value)
		{
			if (value == null)
				return 0;

			double i = 0;
			if (double.TryParse(value.ToString(), out i))
				return i;
			else
				return 0;
		}

		/// <summary>
		/// Converts the string value to its base 64 representation. If the string is null or 
		/// empty then an empty string is returned.
		/// </summary>
		public static string ToBase64(this string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
				return Convert.ToBase64String(Encoding.Default.GetBytes(value));
			else
				return "";
		}

		/// <summary>
		/// Converts the base 64 string back to its original value. If the string is null or 
		/// empty then an empty string is returned.
		/// </summary>
		public static string FromBase64(this string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
				return Encoding.Default.GetString(Convert.FromBase64String(value));
			else
				return "";
		}

		/// <summary>
		/// Attempts to get the object value from the ViewDataDictionary as a string, or if it's null returns an empty string.
		/// </summary>
		public static string GetValue(this ViewDataDictionary dictionary, string key)
		{
			object value = dictionary[key];
			if (value != null)
				return value.ToString();
			else
				return "";
		}

		/// <summary>
		/// Attempts to get the object value from the ViewDataDictionary as a string, or if it's null returns an empty string.
		/// </summary>
		public static string GetValue(this TempDataDictionary dictionary, string key)
		{
			object value = dictionary[key];
			if (value != null)
				return value.ToString();
			else
				return "";
		}

		/// <summary>
		/// Removes any instance of 'vstfs:///VersionControl/Changeset/' from the string.
		/// </summary>
		public static string ParseChangesetLink(this string value)
		{
			// vstfs:///VersionControl/Changeset/28
			return value.Replace("vstfs:///VersionControl/Changeset/", "");
		}

		/// <summary>
		/// Retrieves the value of a [Description] attribute that decorates a property.
		/// </summary>
		public static string GetDescription(this Enum value)
		{
			Type type = value.GetType();
			FieldInfo fieldInfo = type.GetField(value.ToString());

			// Look for a description attribute
			DescriptionAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

			// Return the description or if there isn't one, the value
			return (attribs.Length > 0) ? attribs[0].Description : Enum.GetName(value.GetType(),value);
		}

		/// <summary>
		/// Creates a new <see cref="List`AllowedValuesCollection"/> from a <see cref="AllowedValuesCollection"/>.
		/// </summary>
		public static IList<string> ToList(this AllowedValuesCollection collection)
		{
			List<string> list = new List<string>();

			foreach (string item in collection)
			{
				list.Add(item);
			}

			return list;
		}

		/// <summary>
		/// Converts a <see cref="WorkItemCollection"/> to an <see cref="IEnuermable`WorkItemSummary"/>
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		public static IEnumerable<WorkItemSummary> ToSummaries(this WorkItemCollection collection)
		{
			List<WorkItemSummary> list = new List<WorkItemSummary>();

			foreach (WorkItem item in collection)
			{
				WorkItemSummary summary = WorkItemSummaryFactory.GetForType(item.Type);
				summary.FromWorkItem(item);
				list.Add(summary);
			}

			return list;
		}

		/// <summary>
		/// Populates the core fields of a <see cref="WorkItem"/> from the values of the provided <see cref="WorkItemSummary"/>
		/// </summary>
		public static void FillCoreFields(this WorkItem item, WorkItemSummary summary)
		{
			item.Title = summary.Title;
			item.Description = summary.Description; // TODO: change to appropriate Field
			item.Fields["Assigned To"].Value = summary.AssignedTo;
			item.Fields["State"].Value = summary.State;
			item.IterationPath = summary.IterationPath;
			item.AreaPath = summary.AreaPath;

			if (item.Fields.Contains("Reason"))
			{
				item.Fields["Reason"].Value = summary.Reason;
			}
		}

		/// <summary>
		/// Creates a CSV string (including escaping for commas) from the provided <see cref="WorkItemSummary"/>
		/// </summary>
		public static string ToCsv(this WorkItemSummary summary)
		{
			StringBuilder builder = new StringBuilder();

			// Title
			string title = EscapeQuotes(summary.Title);
			if (title.IndexOf(",") > -1)
				builder.Append("\"" + title + "\"");
			else
				builder.Append(title);

			builder.Append(",");

			// ID
			builder.Append(summary.Id);
			builder.Append(",");

			// Assigned to
			string assignedTo = EscapeQuotes(summary.AssignedTo);
			if (assignedTo.IndexOf(",") > -1)
				builder.Append("\"" + assignedTo + "\"");
			else
				builder.Append(assignedTo);

			builder.Append(",");

			// Created on
			builder.Append(summary.CreatedDate.ToString("ddd dd MMM yyyy"));
			builder.Append(",");

			// State
			string state = EscapeQuotes(summary.State);
			if (state.IndexOf(",") > -1)
				builder.Append("\"" + state + "\"");
			else
				builder.Append(state);

			builder.Append(",");

			return builder.AppendLine().ToString();
		}

		public static string EscapeQuotes(string text)
		{
			return text.Replace("\"", "\"\"");
		}
	}
}
