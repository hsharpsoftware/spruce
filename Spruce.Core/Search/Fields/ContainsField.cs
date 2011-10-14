using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	/// <summary>
	/// Changes the Field class to use a CONTAINS search.
	/// </summary>
	public class ContainsField : Field
	{
		public override string ToString()
		{
			// The WIQL version of a LIKE/full text query
			return string.Format("{0} {1}CONTAINS @{2}", ColumnName, (IsNot) ? "NOT " : "", ParameterName);
		}
	}
}
