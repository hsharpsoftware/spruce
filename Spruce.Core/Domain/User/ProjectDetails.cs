
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;

namespace Spruce.Core
{
    /// <summary>
    /// Contains information for a project, including its id, name and its valid areas and iterations.
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
			Users = new List<string>();
			Areas = new List<AreaSummary>();
			Iterations = new List<IterationSummary>();
			StoredQueries = new List<StoredQuerySummary>();

			AddWorkItemTypes();
			AddWorkItemTypesAsStrings();
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

		private void AddStoredQueries()
		{
			foreach (StoredQuery item in _project.StoredQueries)
			{
				StoredQueries.Add(new StoredQuerySummary() { Id = item.QueryGuid, Name = item.Name });
			}
		}
	}
}