using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Core
{
	public class WorkItemTemplate
	{
		public IList<string> ValidStates { get; set; }
		public IList<string> ValidReasons { get; set; }

		public WorkItemTemplate()
		{
			ValidStates = new List<string>();
			ValidReasons = new List<string>();
		}

		public virtual void PopulateAllowedValues(WorkItem item)
		{
			// All valid states
			ValidStates = new List<string>();
			if (item.Fields.Contains("State"))
			{
				ValidStates = item.Fields["State"].AllowedValues.ToList();
			}
			// All valid reasons
			ValidReasons = new List<string>();
			if (item.Fields.Contains("Reason"))
			{
				// Use FieldDefinitions.AllowedValues not the AllowedValues as they're always empty.
				if (item.IsNew)
					ValidReasons = item.Fields["Reason"].AllowedValues.ToList();
				else
					ValidReasons = item.Fields["Reason"].FieldDefinition.AllowedValues.ToList();
			}
		}
	}
}