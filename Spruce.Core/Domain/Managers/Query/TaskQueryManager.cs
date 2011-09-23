using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	public class TaskQueryManager : QueryManager
	{
		public TaskQueryManager()
			: base()
		{
			base.ContrainType("Task");
		}

		public IEnumerable<string> GetAllowedValuesForState(string state, string fieldName)
		{
			return base.GetAllowedValuesForState(UserContext.Current.CurrentProject.WorkItemTypeForTask, state, fieldName);
		}
	}
}
