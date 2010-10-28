using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spruce.Models
{
	public class FilterSettings
	{
		public string AreaPath { get; set; }
		public string IterationPath { get; set; }

		/// <summary>
		/// A comma separated list of states, e.g. active,resolved. An empty value
		/// denotes any states.
		/// </summary>
		public string States { get; set; }
	}
}