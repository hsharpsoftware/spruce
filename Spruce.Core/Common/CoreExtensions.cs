using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	public static class CoreExtensions
	{
		/// <summary>
		/// Attempts to parse the object as a string value and convert to an integer. If this fails, zero is returned.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int ToIntOrDefault(this object value)
		{
			if (value == null)
				return 0;

			int i = 0;
			if (int.TryParse(value.ToString(),out i))
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
	}
}
