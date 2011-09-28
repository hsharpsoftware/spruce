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
	public static class HtmlExtensions
	{
		public static string ToCsv(this BugSummary summary)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(ToCsv(summary));

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

		public static string ToCsv(this TaskSummary summary)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(ToCsv(summary));

			// Priority
			builder.Append(summary.Priority);
			builder.Append(",");

			return builder.AppendLine().ToString();
		}
	}
}
