using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	public class BugQueryManager : QueryManager
	{
		public BugQueryManager(): base()
		{
			base.ContrainType("Bug");
		}

		public IEnumerable<string> GetAllowedValuesForState(string state, string fieldName)
		{
			return base.GetAllowedValuesForState(UserContext.Current.CurrentProject.WorkItemTypeForBug, state, fieldName);
		}
	}
}
