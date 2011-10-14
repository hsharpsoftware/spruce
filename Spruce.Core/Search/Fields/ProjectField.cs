using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	/// <summary>
	/// A project name search, which supports '*' to represent all projects.
	/// </summary>
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
