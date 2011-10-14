using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spruce.Core
{
	/// <summary>
	/// Contains information about a stored query, for use in an MVC view or controller (this is the 'Model').
	/// </summary>
	public class StoredQuerySummary
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
	}
}