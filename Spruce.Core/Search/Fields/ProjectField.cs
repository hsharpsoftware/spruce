using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	public class ProjectField : Field
	{
		public override string ToString()
		{
			// A value of "*" for project: means all projects, so just leave out field
			if (Value.ToString() == "*")
				return "";
			else
				return base.ToString();
		}
	}
}
