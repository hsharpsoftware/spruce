using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Reflection;

namespace Spruce.Core
{
	public class WorkItemSummaryFactory
	{
		private static bool _hasScanned;
		private static Dictionary<string, Type> _types;

		public static bool HasScanned()
		{
			return _hasScanned;
		}

		public static void Scan()
		{
			_hasScanned = true; // avoid a StackOverflow: instance.WorkItemType (below) calls Scan() via the UserContext
			_types = new Dictionary<string, Type>();

			Type summaryType = typeof(WorkItemSummary);
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(summaryType))
					{
						WorkItemSummary instance = (WorkItemSummary) type.Assembly.CreateInstance(type.FullName);

						if (instance.WorkItemType == null)
							throw new NullReferenceException(string.Format("The {0} provided has a null WorkItemType. This is typically because its WIQLTypeName does not match the name assigned in TFS.",instance));

						_types.Add(instance.WorkItemType.Name,type);
					}
				}
			}
		}

		public static WorkItemSummary GetForType(WorkItemType workItemType)
		{
			if (!_types.ContainsKey(workItemType.Name))
				throw new InvalidOperationException(string.Format("{0} has no equivalent WorkItemSummary. This is typically because its WIQLTypeName does not match the name assigned in TFS.",workItemType.Name));

			Type type = _types[workItemType.Name];	
			return Activator.CreateInstance(type, false) as WorkItemSummary;
		}
	}
}
