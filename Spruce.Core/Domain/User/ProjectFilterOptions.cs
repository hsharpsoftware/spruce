using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Web;

namespace Spruce.Core
{
	/// <summary>
	/// Contains a set of <see cref="FilterOptions"/> for all views for a specific project.
	/// </summary>
	public class ProjectFilterOptions
	{
		/// <summary>
		/// The project name these filter options are for.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A dictionary of <see cref="FilterOptions"/> where the key is the view name (e.g. bugs).
		/// This contains the filter options for a particular view - do not use this property for retrieving 
		/// the filter options, use <see cref="GetByKey"/> instead, as this property is for serialization only.
		/// </summary>
		public SerializableDictionary<string,FilterOptions> FilterOptions { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectFilterOptions"/> class.
		/// </summary>
		/// <remarks>Constructor for serialization</remarks>
		public ProjectFilterOptions()
		{
			FilterOptions = new SerializableDictionary<string, FilterOptions>();
		}

		public ProjectFilterOptions(string projectName) : this()
		{
			Name = projectName;		
		}

		/// <summary>
		/// Retrieves the <see cref="FilterOptions"/> for the named view
		/// </summary>
		public FilterOptions GetByKey(string viewName)
		{
			if (!FilterOptions.ContainsKey(viewName))
				FilterOptions.Add(viewName, new FilterOptions());

			return FilterOptions[viewName];
		}

		/// <summary>
		/// Updates the filter options for the view.
		/// </summary>
		public void UpdateFilterOption(string viewName, FilterOptions options)
		{
			if (!FilterOptions.ContainsKey(viewName))
				FilterOptions.Add(viewName, new FilterOptions());

			FilterOptions[viewName] = options;

			UserContext.Current.Settings.Save();
		}
	}
}
