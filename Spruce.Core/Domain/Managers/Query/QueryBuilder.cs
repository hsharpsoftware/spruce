using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	public class QueryBuilder
	{
		protected string _query;
		protected Dictionary<string, object> _parameters;
		protected List<string> _orFilters;
		protected List<string> _andFilters;

		public Dictionary<string, object> Parameters 
		{ 
			get { return _parameters; } 
		}

		public QueryBuilder()
		{
			_parameters = new Dictionary<string, object>();
			_parameters.Add("project", UserContext.Current.CurrentProject.Name);
			_orFilters = new List<string>();
			_andFilters = new List<string>();

			_query = string.Format("SELECT ID, Title from Issue WHERE " +
				"System.TeamProject = @project {0} %FILTERS% " +
				"ORDER BY Id DESC", AddSqlForPaths(_parameters));
		}

		public void And(string query)
		{
			_andFilters.Add(query);
		}

		public void Or(string query)
		{
			_orFilters.Add(query);
		}

		public void AddParameter(string name,object value)
		{
			_parameters.Add(name, value);
		}

		private string AddSqlForPaths(Dictionary<string, object> parameters)
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
