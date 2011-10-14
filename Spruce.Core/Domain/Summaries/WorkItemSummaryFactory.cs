using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Reflection;

namespace Spruce.Core
{
	/// <summary>
	/// Scans all assemblies in the current appdomain, looking for classes that derive from <see cref="WorkItemSummary"/>.
	/// When found, these are cached so that they can be used actions for the <see cref="WorkItemType"/> they represent.
	/// </summary>
	public class WorkItemSummaryFactory
	{
		private static bool _hasScanned;
		private static Dictionary<string, Type> _types;

		public static bool HasScanned()
		{
			return _hasScanned;
		}

		/// <summary>
		/// Scans all assemblies for <see cref="WorkItemSummary"/> derived classes. This should be called once at app startup.
		/// </summary>
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

		/// <summary>
		/// Retrieves the <see cref="WorkItemSummary"/> derived class that is used for the <see cref="WorkItemType"/>.
		/// </summary>
		public static WorkItemSummary GetForType(WorkItemType workItemType)
		{
			if (!_types.ContainsKey(workItemType.Name))
			{
				Log.Warn("{0} has no equivalent WorkItemSummary. This is typically because its WIQLTypeName does not match the name assigned in TFS.", workItemType.Name);
				return new WorkItemSummary();
			}

			Type type = _types[workItemType.Name];	
			return Activator.CreateInstance(type, false) as WorkItemSummary;
		}
	}
}
