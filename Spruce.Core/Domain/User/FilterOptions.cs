using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	public class FilterOptions
	{
		public bool Active { get; set; }
		public bool Resolved { get; set; }
		public bool Closed { get; set; }
		public bool AssignedToMe { get; set; }
		public bool Today { get; set; }
		public bool Yesterday { get; set; }
		public bool ThisWeek { get; set; }
		public bool ThisMonth { get; set; }
		public bool LastMonth { get; set; }
	}
}
