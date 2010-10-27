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
		public ItemState States { get; set; }

		[Flags]
		public enum ItemState
		{
			Closed = 0,
			Resolved = 1,
			Active = 2,
			All = Closed | Resolved | Active
		}
	}
}