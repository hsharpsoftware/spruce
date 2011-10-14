using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Spruce.Core
{
	/// <summary>
	/// Contains all filter options for lists of <see cref="WorkItemSummarys"/>
	/// </summary>
	public class FilterOptions
	{
		public bool Active { get; set; }
		public bool Resolved { get; set; }
		public bool Closed { get; set; }

		public string Title { get; set; }
		public string AssignedTo { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterOptions"/> class.
		/// </summary>
		public FilterOptions()
		{
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;
		}

		/// <summary>
		/// Creates a new <see cref="FilterOptions"/> object using the parameters passed.
		/// </summary>
		/// <param name="title">The title to filter by.</param>
		/// <param name="assignedTo">The assigned user to filter by.</param>
		/// <param name="startDate">The start date to filter by.</param>
		/// <param name="endDate">The end date to filter by.</param>
		/// <param name="status">The status to filter by.</param>
		/// <returns></returns>
		public static FilterOptions Parse(string title, string assignedTo, string startDate, string endDate, string status)
		{
			FilterOptions filterOptions = new FilterOptions();
			filterOptions.Title = title;

			// Assigned to, this could be expanded to more names
			if (assignedTo == "me")
				assignedTo = UserContext.Current.Name;
			
			filterOptions.AssignedTo = assignedTo;

			// Dates
			if (!string.IsNullOrEmpty(startDate))
			{
				filterOptions.StartDate = FromNamedDate(startDate);
			}

			if (!string.IsNullOrEmpty(endDate))
			{
				filterOptions.EndDate = FromNamedDate(endDate);
			}

			// Status
			if (!string.IsNullOrEmpty(status) && status != "All")
			{
				status = status.ToLower();

				switch (status)
				{
					case "resolved":
						filterOptions.Resolved = true;
						break;

					case "closed":
						filterOptions.Closed = true;
						break;

					case "active":
					default:
						filterOptions.Active = true;
						break;
				}
			}

			return filterOptions;
		}

		/// <summary>
		/// Returns the current object instance's status as a string.
		/// </summary>
		/// <returns></returns>
		public string ConvertStatusToString()
		{
			if (Active)
				return "Active";
			if (Closed)
				return "Closed";
			if (Resolved)
				return "Resolved";

			return "All";
		}

		private static DateTime FromNamedDate(string namedDate)
		{
			if (string.IsNullOrEmpty(namedDate))
				return DateTime.MinValue;

			namedDate = namedDate.ToLower();

			if (namedDate == "today")
			{
				return DateTime.Today;
			}
			else if (namedDate == "yesterday")
			{
				return DateTime.Today.Yesterday();
			}
			else if (namedDate == "thisweek")
			{
				return DateTime.Today.StartOfWeek();
			}
			else if (namedDate == "thismonth")
			{
				return DateTime.Today.StartOfThisMonth();
			}
			else if (namedDate == "lastmonth")
			{
				return DateTime.Today.StartOfLastMonth();
			}
			else
			{
				DateTime dateTime = DateTime.MinValue;
				if (!DateTime.TryParse(namedDate, out dateTime))
					dateTime = DateTime.MinValue;

				return dateTime;
			}
		}
	}
}
