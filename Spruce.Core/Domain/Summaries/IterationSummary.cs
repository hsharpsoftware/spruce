using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spruce.Core
{
	/// <summary>
	/// Contains information about an iteration, for use in an MVC view or controller (this is the 'Model').
	/// </summary>
	public class IterationSummary
	{
		public string Name { get; set; }
		public string Path { get; set; }
	}
}