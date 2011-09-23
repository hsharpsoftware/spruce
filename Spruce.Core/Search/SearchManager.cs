using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace Spruce.Core.Search
{
	/// <summary>
	/// 
	/// </summary>
	public class SearchManager
	{
		public bool IsId(string query)
		{
			int i;
			return int.TryParse(query, out i);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IEnumerable<WorkItemSummary> Search(string query)
		{
			IEnumerable<WorkItemSummary> summaries = new List<WorkItemSummary>();
			QueryManager manager = new QueryManager();

			// Is it just a number? then it's an id
			if (IsId(query))
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				parameters.Add("Id",Convert.ToInt32(query));
				string wiql = "SELECT * FROM Issues WHERE [System.Id] = @Id";

				return manager.ExecuteWiqlQuery(wiql, parameters, true);
			}
			else
			{
				QueryParser parser = new QueryParser();
				parser.ParseComplete += delegate(object sender, EventArgs e)
				{
					Dictionary<string, object> parameters = new Dictionary<string, object>();
					string wiql = parser.WiqlBuilder.BuildQuery(parameters);

					summaries = manager.ExecuteWiqlQuery(wiql, parameters, true);
				};

				parser.SearchFor(query);
				return summaries;
			}
		}
	}
}
