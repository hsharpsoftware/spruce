using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Linq;

namespace Spruce.Core
{
	//[DebuggerDisplay("Type={GetType()},Title={Title}")]
	public class WorkItemSummary
	{
		private WorkItemType _workItemType;

		public int Id { get; set; }
		public string ProjectName { get; set; }
		public bool IsNew { get; set; }

		/// <summary>
		/// The name used for WIQL Querying, which is required to be overriden by inheriting classes.
		/// </summary>
		public virtual string WIQLTypeName { get; protected set; }

		/// <summary>
		/// The name of the controller used to view/edit this item
		/// </summary>
		public virtual string Controller { get; protected set; }

		public string AreaId { get; set; }
		public string AreaPath { get; set; }
		public string IterationId { get; set; }
		public string IterationPath { get; set; }

		public string Title { get; set; }
		public string Description { get; set; }

		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public string AssignedTo { get; set; }
		public string AssignedBy { get; set; }
		
		public string State { get; set; }
		public string Reason { get; set; }

		public FieldCollection Fields { get; internal set; }
		public AttachmentCollection Attachments { get; set; }
		public LinkCollection Links { get; set; }
		public RevisionCollection Revisions { get; set; }

		public IList<string> ValidStates { get; set; }
		public IList<string> ValidReasons { get; set; }

		public WorkItemType WorkItemType
		{
			get
			{
				if (_workItemType == null)
				{
					// TODO: Optimize this with a cache
					if (string.IsNullOrEmpty(WIQLTypeName))
						throw new NullReferenceException("The WIQLTypeName property is null or empty, and is required for the WorkItemType");

					_workItemType = UserContext.Current.CurrentProject.WorkItemTypes.FirstOrDefault(t => t.Name == WIQLTypeName);
				}

				return _workItemType;
			}
		}

		public virtual void FromWorkItem(WorkItem item)
		{
			Id = item.Id;
			AssignedTo = item.Fields["Assigned To"].Value.ToString();
			CreatedDate = item.CreatedDate;
			CreatedBy = item.CreatedBy;
			AreaId = item.AreaPath.Replace(item.Project.Name + "\\", "");
			AreaPath = item.AreaPath;
			Description = item.Description;
			IterationId = item.IterationPath.Replace(item.Project.Name + "\\", "");
			IterationPath = item.IterationPath;
			State = item.State;
			Title = item.Title;
			ProjectName = item.Project.Name;
			Fields = item.Fields;
			Attachments = item.Attachments;
			Links = item.Links;
			Revisions = item.Revisions;

			if (item.Fields.Contains("Reason"))
				Reason = item.Fields["Reason"].Value.ToString();

			// For CMMI projects - turn the description into the Repro fields
			if (item.Fields.Contains("Repro Steps") && string.IsNullOrEmpty(item.Description))
				Description = GetFieldValue(item, "Repro Steps");
		}

		public virtual WorkItem ToWorkItem()
		{
			throw new NotImplementedException("ToWorkItem must be overridden by an inheriting class");
		}

		protected void FillCoreFieldsFromSummary(WorkItem item)
		{
			item.Title = Title;
			item.Description = Description; // TODO: change to appropriate Field
			item.AreaPath = AreaPath;

			item.Fields["Assigned To"].Value = AssignedTo;
			item.Fields["State"].Value = State;
			item.IterationPath = IterationPath;

			if (item.Fields.Contains("Reason"))
			{
				item.Fields["Reason"].Value = Reason;
			}

			// For CMMI projects (redundant?)
			if (item.Fields.Contains("Repro Steps"))
				item.Fields["Repro Steps"].Value = Description;

			if (item.Fields.Contains("Symptom"))
				item.Fields["Symptom"].Value = Description;
		}

		/// <summary>
		/// Accomodates fields that won't necessarily exist (such as resolved by) until a later stage of the work item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		protected string GetFieldValue(WorkItem item, string fieldName)
		{
			if (item.Fields.Contains(fieldName))
				return Convert.ToString(item.Fields[fieldName].Value);
			else
				return "";
		}

		public virtual void PopulateAllowedValues(WorkItem item)
		{
			ValidStates = new List<string>();
			ValidReasons = new List<string>();

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