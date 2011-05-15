﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

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

		public static string GetDescription(this Enum value)
		{
			Type type = value.GetType();
			FieldInfo fieldInfo = type.GetField(value.ToString());

			// Look for a description attribute
			DescriptionAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

			// Return the description or if there isn't one, the value
			return (attribs.Length > 0) ? attribs[0].Description : Enum.GetName(value.GetType(),value);
		}
	}
}
