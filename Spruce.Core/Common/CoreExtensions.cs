using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public static class CoreExtensions
	{
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
		/// Attempts to parse the object as a string value and convert to an integer. If this fails, zero is returned.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
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

		public static double ToDoubleOrDefault(this object value)
		{
			if (value == null)
				return 0;

			double i = 0;
			if (double.TryParse(value.ToString(), out i))
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

		public static string FromBase64(this string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
				return Encoding.Default.GetString(Convert.FromBase64String(value));
			else
				return "";
		}

		/// <summary>
		/// Converts the object value to a string, or if it's null returns an empty string.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string SafeToString(this object value)
		{
			if (value != null)
				return value.ToString();
			else
				return "";
		}

		public static string ParseChangesetLink(this string value)
		{
			// vstfs:///VersionControl/Changeset/28
			return value.Replace("vstfs:///VersionControl/Changeset/", "");
		}

		public static string GetDescription(this Enum value)
		{
			Type type = value.GetType();
			FieldInfo fieldInfo = type.GetField(value.ToString());

			// Look for a description attribute
			DescriptionAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

			// Return the description or if there isn't one, the value
			return (attribs.Length > 0) ? attribs[0].Description : Enum.GetName(value.GetType(),value);
		}

		public static IList<string> ToList(this AllowedValuesCollection collection)
		{
			List<string> list = new List<string>();

			foreach (string item in collection)
			{
				list.Add(item);
			}

			return list;
		}

		public static IList<T> ToSummaryList<T>(this WorkItemCollection collection) where T : WorkItemSummary
		{
			List<T> list = new List<T>();

			foreach (WorkItem item in collection)
			{
				T summary = default(T);
				summary.FromWorkItem(item);
				list.Add(summary);
			}

			return list;
		}

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
	}
}
