using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Web;

namespace Spruce.Core
{
	public class ProjectFilterOptions
	{
		/// <summary>
		/// The project name these filter options are for.
		/// </summary>
		public string Name { get; set; }
		public FilterOptions BugFilterOptions { get; set; }
		public FilterOptions BugHeatmapFilterOptions { get; set; }
		public FilterOptions TaskFilterOptions { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFilterOptions"/> class.
		/// </summary>
		/// <remarks>Constructor for serialization</remarks>
		public ProjectFilterOptions()
		{
		}

		public ProjectFilterOptions(string projectName)
		{
			Name = projectName;
			BugFilterOptions = new FilterOptions();
			BugHeatmapFilterOptions = new FilterOptions();
			TaskFilterOptions = new FilterOptions();
		}
	}
}
