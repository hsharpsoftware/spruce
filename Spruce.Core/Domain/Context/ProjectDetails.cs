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

		public WorkItemType WorkItemTypeForBug { get; private set; }
		public WorkItemType WorkItemTypeForTask { get; private set; }

		public ProjectDetails(Project project)
		{
			_project = project;
			Id = project.Id;
			Name = project.Name;
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

			AddWorkItemTypes();
			AddWorkItemTypesAsStrings();
			WorkItemTypeForBug = WorkItemTypes.FirstOrDefault(t => t.Name == "Bug");
			WorkItemTypeForTask = WorkItemTypes.FirstOrDefault(t => t.Name == "Task");
			
			AddReasonsForBug();
			AddReasonsForTask();
			AllowedTriageTypes = GetAllowedValues("Triage");
			AllowedPriorities = GetAllowedValues("Priority");
			AllowedSeverities = GetAllowedValues("Severity");
			AllowedStates = GetAllowedValues("State");

			AddAreas();
			AddIterations();	
		}

		private void AddAreas()
		{
			foreach (Node areaNode in _project.AreaRootNodes)
			{
				Areas.Add(new AreaSummary()
				{
					Name = areaNode.Name,
					Path = areaNode.Path,
				});
			}

			// If there are no subnodes, use the project name
			if (Areas.Count == 0)
			{
				Areas.Add(new AreaSummary()
				{
					Name = _project.Name,
					Path = _project.Name,
				});
			}
		}

		private void AddIterations()
		{
			foreach (Node iterationNode in _project.IterationRootNodes)
			{
				Iterations.Add(new IterationSummary()
				{
					Name = iterationNode.Name,
					Path = iterationNode.Path,
				});
			}

			// If there are no subnodes, use the project name
			if (Iterations.Count == 0)
			{
				Iterations.Add(new IterationSummary()
				{
					Name = _project.Name,
					Path = _project.Name,
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

		private void AddReasonsForBug()
		{
			WorkItemType workItemType = WorkItemTypeForBug;

			foreach (string value in workItemType.FieldDefinitions["Reason"].AllowedValues)
			{
				ReasonsForBug.Add(value);
			}
		}

		private void AddReasonsForTask()
		{
			WorkItemType workItemType = WorkItemTypeForTask;

			foreach (string value in workItemType.FieldDefinitions["Reason"].AllowedValues)
			{
				ReasonsForTask.Add(value);
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
	}
}