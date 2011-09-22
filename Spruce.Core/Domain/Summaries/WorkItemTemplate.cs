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
		public IList<string> ValidPriorities { get; set; }
		public IList<string> ValidSeverities { get; set; }
		public IList<string> ValidReasons { get; set; }

		public WorkItemTemplate()
		{
			ValidStates = new List<string>();
			ValidPriorities = new List<string>();
			ValidSeverities = new List<string>();
			ValidReasons = new List<string>();
		}

		public static void PopulateAllowedValues(WorkItem item)
		{
			WorkItemTemplate template = new WorkItemTemplate();
			// All valid states
			template.ValidStates = new List<string>();
			if (item.Fields.Contains("State"))
			{
				template.ValidStates = item.Fields["State"].AllowedValues.ToList();
			}

			// All valid priorities
			template.ValidPriorities = new List<string>();
			if (item.Fields.Contains("Priority"))
			{
				template.ValidPriorities = item.Fields["Priority"].AllowedValues.ToList();
			}

			// All valid severities
			template.ValidSeverities = new List<string>();
			if (item.Fields.Contains("Severity"))
			{
				template.ValidSeverities = item.Fields["Severity"].AllowedValues.ToList();
			}

			// All valid reasons
			template.ValidReasons = new List<string>();
			if (item.Fields.Contains("Reason"))
			{
				// Use FieldDefinitions.AllowedValues not the AllowedValues as they're always empty.
				if (item.IsNew)
					template.ValidReasons = item.Fields["Reason"].AllowedValues.ToList();
				else
					template.ValidReasons = item.Fields["Reason"].FieldDefinition.AllowedValues.ToList();
			}
		}
	}
}