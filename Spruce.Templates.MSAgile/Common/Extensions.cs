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

namespace Spruce.Templates.MSAgile
{
	/// <summary>
	/// Core extensions for the MGAgile template.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Produces a CSV represenation of the provided <see cref="BugSummary"/>.
		/// </summary>
		public static string ToCsv(this BugSummary summary)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(((WorkItemSummary)summary).ToCsv());

			// Priority
			builder.Append(summary.Priority);
			builder.Append(",");

			// Severity
			string severity = summary.Severity.Replace("\"", "\"\"");

			if (severity.IndexOf(",") > -1)
				builder.Append("\"" + severity + "\"");
			else
				builder.Append(severity);

			builder.Append(",");

			return builder.AppendLine().ToString();
		}

		/// <summary>
		/// Produces a CSV represenation of the provided <see cref="TaskSummary"/>.
		/// </summary>
		public static string ToCsv(this TaskSummary summary)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(((WorkItemSummary)summary).ToCsv());

			// Priority
			builder.Append(summary.Priority);
			builder.Append(",");

			return builder.AppendLine().ToString();
		}
	}
}
