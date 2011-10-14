using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	/// <summary>
	/// A search for a date field. This class attempts to convert a string representation of 
	/// the date into its DateTime counterpart, including human readable date names like "yesterday".
	/// </summary>
	public class DateField : Field
	{
		public override object Value
		{
			get
			{
				return ConvertFromReadableDate(base.Value.ToString());
			}
			set
			{
				base.Value = value;
			}
		}

		private DateTime ConvertFromReadableDate(string text)
		{
			text = text.ToLower();
			DateTime dateTime = DateTime.Today;

			if (text == "today")  dateTime = DateTime.Today;
			else if (text == "yesterday")  dateTime = DateTime.Now.Yesterday();
			else if (text == "thisweek")  dateTime = DateTime.Today.StartOfWeek();
			else if (text == "lastweek")  dateTime = DateTime.Today.LastWeek();
			else if (text == "thismonth") dateTime = DateTime.Today.StartOfThisMonth();
			else if (text == "lastmonth") dateTime = DateTime.Today.StartOfLastMonth();
			else
			{
				if (!DateTime.TryParse(text, out dateTime))
					dateTime = DateTime.Today;
			}

			return dateTime;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} @{2}", ColumnName, (IsNot) ? "<>" : ">", ParameterName);
		}
	}
}
