using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Server;

namespace Spruce.Core
{
	/// <summary>
	/// Summary or meta information for a project that is cached (per user - but should be global for the app domain).
	/// </summary>
	public class ProjectDetails
	{
		private Project _project;

		public int Id { get; private set; }
		public string Name { get; private set; }
		public string Path { get; private set; }
		public string TemplateName { get; private set; }
		public List<WorkItemType> WorkItemTypes { get; private set; }
		public List<string> WorkItemTypesAsStrings { get; private set; }
		public List<string> ReasonsForBug { get; private set; }
		public List<string> ReasonsForTask { get; private set; }
		public List<string> AllowedTriageTypes { get; private set; }
		public List<string> AllowedPriorities { get; private set; }
		public List<string> AllowedSeverities { get; private set; }
		public List<string> AllowedStates { get; private set; }
		public List<string> Users { get; private set; }
		public IList<AreaSummary> Areas { get; internal set; }
		public IList<IterationSummary> Iterations { get; private set; }
		public IList<StoredQuerySummary> StoredQueries { get; private set; }

		public ProjectDetails(Project project)
		{
			_project = project;
			Id = project.Id;
			Name = project.Name;
			Path = "$/" + Name; // TODO: check this is correct.
			TemplateName = "";

			WorkItemTypes = new List<WorkItemType>();
			WorkItemTypesAsStrings = new List<string>();
			ReasonsForBug = new List<string>();
			ReasonsForTask = new List<string>();
			AllowedTriageTypes = new List<string>();
			AllowedPriorities = new List<string>();
			AllowedSeverities = new List<string>();
			Users = new List<string>();
			Areas = new List<AreaSummary>();
			Iterations = new List<IterationSummary>();
			StoredQueries = new List<StoredQuerySummary>();

			AddWorkItemTypes();
			AddWorkItemTypesAsStrings();

			AllowedTriageTypes = GetAllowedValues("Triage");
			AllowedPriorities = GetAllowedValues("Priority");
			AllowedSeverities = GetAllowedValues("Severity");
			AllowedStates = GetAllowedValues("State");

			AddAreas();
			AddIterations();
			AddStoredQueries();
		}

		private void AddAreas()
		{
			// Add a "none" option - TFS stores the iteration name as the project name
			Areas.Add(new AreaSummary()
			{
				Name = "None",
				Path = Name,
			});

			foreach (Node areaNode in _project.AreaRootNodes)
			{
				Areas.Add(new AreaSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}
		}

		private void AddIterations()
		{
			// Add a "none" option - TFS stores the iteration name as the project name
			Iterations.Add(new IterationSummary()
			{
				Name = "None",
				Path = Name,
			});

			foreach (Node iterationNode in _project.IterationRootNodes)
			{
				Iterations.Add(new IterationSummary()
				{
					Name = iterationNode.Name,
					Path = iterationNode.Path,
				});
			}
		}


		private void AddWorkItemTypes()
		{
			foreach (WorkItemType workItemType in _project.WorkItemTypes)
			{
				WorkItemTypes.Add(workItemType);
			}
		}

		private void AddWorkItemTypesAsStrings()
		{
			foreach (WorkItemType workItemType in _project.WorkItemTypes)
			{
				WorkItemTypesAsStrings.Add(workItemType.Name);
			}
		}

		private List<string> GetAllowedValues(string fieldName)
		{
			List<string> list = new List<string>();

			foreach (FieldDefinition definition in _project.Store.FieldDefinitions)
			{
				if (definition.Name == fieldName)
				{
					foreach (string value in definition.AllowedValues)
					{
						list.Add(value);
					}
				}
			}

			return list;
		}

		private void AddStoredQueries()
		{
			foreach (StoredQuery item in _project.StoredQueries)
			{
				StoredQueries.Add(new StoredQuerySummary() { Id = item.QueryGuid, Name = item.Name });
			}
		}
	}
}