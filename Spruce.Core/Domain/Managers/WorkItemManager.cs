using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;
using System.Net;
using System.Security.Principal;
using Microsoft.TeamFoundation.Server;
using System.Xml;
using Microsoft.TeamFoundation;
using System.Web.Mvc;
using System.Text;

namespace Spruce.Core
{
	public abstract class WorkItemManager
	{
		public void Save(WorkItemSummary summary)
		{
			WorkItem item = summary.ToWorkItem();

			try
			{			
				item.Save();
				summary.Id = item.Id;
			}
			catch (ValidationException e)
			{
				StringBuilder builder = new StringBuilder();
				foreach (Field field in item.Fields)
				{
					if (field.Status != FieldStatus.Valid)
						builder.AppendLine(string.Format("The '{0}' field has the status {1}", field.Name, field.Status));
				}

				throw new SaveException(string.Format("Save failed for '{0}' ({1}).\nFields: {2}", item.Title,e.Message,builder), e);
			}
		}

		public void SaveAttachments(int id, IEnumerable<Attachment> attachments)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);
			foreach (Attachment attachment in attachments)
			{
				item.Attachments.Add(attachment);
			}

			try
			{
				item.Save();
			}
			catch (ValidationException e)
			{
				throw new SaveException(string.Format("Unable to save attachments for '{0}' ({1})", item.Title, e.Message), e);
			}
		}

		public virtual void Resolve(int id)
		{
			QueryManager queryManager = new QueryManager();
			try
			{
				WorkItemSummary summary = queryManager.ItemById(id);
				summary.State = "Resolved";
				Save(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public virtual void Close(int id)
		{
			QueryManager manager = new QueryManager();
			try
			{
				WorkItemSummary summary = manager.ItemById(id);
				summary.State = "Closed";
				Save(summary);
			}
			catch (Exception)
			{
				// TODO: log invalid state
			}
		}

		public void DeleteAttachment(int id, string url)
		{
			WorkItem item = UserContext.Current.WorkItemStore.GetWorkItem(id);
			if (item.Attachments.Count > 0)
			{
				int index = -1;

				// Appears that there is no smarter way of doing this with AttachmentCollection
				for (int i = 0; i < item.Attachments.Count; i++)
				{
					if (item.Attachments[i].Uri.ToString() == url)
					{
						index = i;
						break;
					}
				}

				if (index > -1)
				{

					try
					{
						item.Attachments.RemoveAt(index);
						item.Save();
					}
					catch (ValidationException e)
					{
						throw new SaveException(string.Format("Removing attachment {0} failed for id '{1}' ({2})", url, id, e.Message), e);
					}
				}
				else
				{
					// TODO: warn
				}
			}
		}

		public abstract WorkItemSummary NewItem();

		protected virtual WorkItem CreateWorkItem(WorkItemType type, WorkItemSummary summary)
		{
			WorkItem item = new WorkItem(type);

			summary.CreatedBy = UserContext.Current.Name;
			summary.AssignedTo = UserContext.Current.Name;
			summary.Fields = item.Fields;
			summary.IsNew = true;

			// Default area + iteration
			summary.AreaPath = UserContext.Current.Settings.AreaPath;
			summary.IterationPath = UserContext.Current.Settings.IterationPath;

			summary.PopulateAllowedValues(item);
			summary.State = summary.ValidStates[0];
			summary.Reason = summary.ValidReasons[0];

			return item;
		}
	}
}