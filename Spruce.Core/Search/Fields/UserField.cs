using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	/// <summary>
	/// A user search field. User names can be comma separated.
	/// </summary>
	public class UserField : Field
	{
		public override object Value
		{
			get
			{
				string val = base.Value.ToString();
				if (!string.IsNullOrEmpty(val) && val.Contains(","))
				{
					string[] names = val.Split(',');
					return "'" + string.Join("','", names) + "'";
				}
				else
				{
					return base.Value;
				}
			}
			set
			{
				base.Value = value;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1}IN (@{2})", ColumnName, (IsNot) ? "NOT " : "", ParameterName);
		}
	}
}
