using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Spruce.Core.Search
{
	/// <summary>
	/// 
	/// </summary>
	public class Field
	{
		/// <summary>
		/// Used for the @name in the parameters
		/// </summary>

		public virtual string ParameterName { get; set; }

		/// <summary>
		/// Gets or sets the name of the column.
		/// </summary>
		public virtual string ColumnName { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		public virtual object Value { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public virtual bool IsNot { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			// Check if project:* and then simply don't add.
			return string.Format("{0} {1} @{2}", ColumnName, (IsNot) ? "!=" : "=", ParameterName);
		}
	}
}
