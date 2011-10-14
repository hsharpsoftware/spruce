using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	/// <summary>
	/// Represents a single WIQL query for a work item type, with very basic AND/OR handling. 
	/// </summary>
	public class QueryBuilder
	{
		protected string _query;
		protected Dictionary<string, object> _parameters;
		protected List<string> _orFilters;
		protected List<string> _andFilters;

		/// <summary>
		/// All parameters that the WIQL references.
		/// </summary>
		public Dictionary<string, object> Parameters 
		{ 
			get { return _parameters; } 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryBuilder"/> class.
		/// </summary>
		public QueryBuilder()
		{
			_parameters = new Dictionary<string, object>();
			_parameters.Add("project", UserContext.Current.CurrentProject.Name);
			_orFilters = new List<string>();
			_andFilters = new List<string>();

			_query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} %FILTERS% " +
				"ORDER BY Id DESC", GenerateWiqlForPaths(_parameters));
		}

		/// <summary>
		/// Appends a query that is appended using an AND.
		/// </summary>
		public void And(string query)
		{
			_andFilters.Add(query);
		}

		/// <summary>
		/// Appends a query that is appended using an OR.
		/// </summary>
		public void Or(string query)
		{
			_orFilters.Add(query);
		}

		/// <summary>
		/// Removes the specified query that has is appended with an OR.
		/// </summary>
		public void RemoveOr(string query)
		{
			_orFilters.Remove(query);
		}

		/// <summary>
		/// Removes the specified query that has is appended with an AND.
		/// </summary>
		public void RemoveAnd(string query)
		{
			_andFilters.Remove(query);
		}

		/// <summary>
		/// Adds a parameter with the name and value for the WIQL query. This can be
		/// referenced inside the query using @name.
		/// </summary>
		public void AddParameter(string name,object value)
		{
			_parameters.Add(name, value);
		}

		/// <summary>
		/// Removes the parameter from the WIQL query.
		/// </summary>
		public void RemoveParameter(string name)
		{
			if (_parameters.ContainsKey(name))
				_parameters.Remove(name);
		}

		/// <summary>
		/// Appends the WIQL required to constrain the query for the user's currently selected iteration and area.
		/// </summary>
		public string GenerateWiqlForPaths(Dictionary<string, object> parameters)
		{
			string result = "";

			if (!string.IsNullOrWhiteSpace(UserContext.Current.Settings.IterationPath))
			{
				parameters.Add("iteration", UserContext.Current.Settings.IterationPath);
				result = " AND [Iteration Path]=@iteration";
			}

			if (!string.IsNullOrWhiteSpace(UserContext.Current.Settings.AreaPath))
			{
				parameters.Add("area", UserContext.Current.Settings.AreaPath);
				result += " AND [Area Path]=@area";
			}

			return result;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the generated WIQL query.
		/// </summary>
		public override string ToString()
		{
			string additionalFilters = "";

			// This is very simplistic right now
			if (_orFilters.Count > 1)
			{
				additionalFilters = "("+ string.Join(" OR ", _orFilters) +")";
			}
			else if (_orFilters.Count == 1)
			{
				additionalFilters = _orFilters[0];
			}

			if (_andFilters.Count > 1)
			{
				if (_orFilters.Count > 0)
					additionalFilters += " AND ("+ string.Join(" AND ", _andFilters) +")";
				else
					additionalFilters += string.Join(" AND ", _andFilters);
			}
			else if (_andFilters.Count == 1)
			{
				if (_orFilters.Count > 0)
					additionalFilters += " AND ("+ _andFilters[0] +")";
				else
					additionalFilters += _andFilters[0];
			}

			if (!string.IsNullOrEmpty(additionalFilters))
			{
				_query = _query.Replace("%FILTERS%", "AND (" + additionalFilters + ")");
			}
			else
			{
				_query = _query.Replace("%FILTERS%", "");
			}

			return _query;
		}
	}
}
