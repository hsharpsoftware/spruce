using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	public class FilterOptions
	{
		public bool Active { get; set; }
		public bool Resolved { get; set; }
		public bool Closed { get; set; }

		public string Title { get; set; }
		public string AssignedTo { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public bool Today { get; set; }
		public bool Yesterday { get; set; }
		public bool ThisWeek { get; set; }
		public bool ThisMonth { get; set; }
		public bool LastMonth { get; set; }

		public FilterOptions()
		{
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MaxValue;
		}

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
				startDate = startDate.ToLower();
				if (startDate == "today")
				{
					filterOptions.Today = true;
				}
				else if (startDate == "yesterday")
				{
					filterOptions.Yesterday = true;
				}
				else if (startDate == "thisweek")
				{
					filterOptions.ThisWeek = true;
				}
				else if (startDate == "thismonth")
				{
					filterOptions.ThisMonth = true;
				}
				else if (startDate == "lastmonth")
				{
					filterOptions.LastMonth = true;
				}
				else
				{
					DateTime start = DateTime.MinValue;
					if (DateTime.TryParse(startDate, out start))
						filterOptions.StartDate = start;
				}
			}

			if (!string.IsNullOrEmpty(endDate))
			{
				endDate = endDate.ToLower();
				if (endDate == "today")
				{
					filterOptions.Today = true;
				}
				else if (endDate == "yesterday")
				{
					filterOptions.Yesterday = true;
				}
				else if (endDate == "thisweek")
				{
					filterOptions.ThisWeek = true;
				}
				else if (endDate == "thismonth")
				{
					filterOptions.ThisMonth = true;
				}
				else if (endDate == "lastmonth")
				{
					filterOptions.LastMonth = true;
				}
				else
				{
					DateTime end = DateTime.MinValue;
					if (DateTime.TryParse(endDate, out end))
						filterOptions.EndDate = end;
				}
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
	}
}
