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

		public FilterOptions GetByKey(string key)
		{
			if (!FilterOptions.ContainsKey(key))
				FilterOptions.Add(key, new FilterOptions());

			return FilterOptions[key];
		}

		public void UpdateFilterOption(string key, FilterOptions options)
		{
			if (!FilterOptions.ContainsKey(key))
				FilterOptions.Add(key, new FilterOptions());

			FilterOptions[key] = options;

			UserContext.Current.Settings.Save();
		}
	}
}
