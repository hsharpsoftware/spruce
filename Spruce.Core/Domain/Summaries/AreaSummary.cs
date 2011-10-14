using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Spruce.Core
{
	/// <summary>
	/// Contains information about an area, for use in an MVC view or controller (this is the 'Model').
	/// </summary>
	public class AreaSummary
	{
		public string Name { get; set; }
		public string Path { get; set; }
	}
}