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
	/// <summary>
	/// The base class for all work item related tasks in Spruce. This class does not include searching, 
	/// this is performed by the QueryManager class.
	/// </summary>
	public abstract class WorkItemManager
	{
		/// <summary>
		/// Saves the specified <see cref="WorkItemSummary"/> to TFS, converting it to a <see cref="WorkItem"/> first.
		/// </summary>
		/// <exception cref="SaveException">Thrown if a TFS server-based error occurs with the save.</exception>
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

		/// <summary>
		/// Saves the provided file attachments for a TFS work item.
		/// </summary>
		/// <param name="id">The id of the WorkItem these attachments are for.</param>
		/// <param name="attachments">A collection of file attachments. These files should exist on disk first.</param>
		/// <exception cref="SaveException">Thrown if a TFS server-based error occurs with the save.</exception>
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

		/// <summary>
		/// Changes the state of a work item to 'resolved'.
		/// </summary>
		/// <param name="id">The id of the work item to change the state of.</param>
		public virtual void Resolve(int id)
		{
			QueryManager queryManager = new QueryManager();
			try
			{
				WorkItemSummary summary = queryManager.ItemById(id);
				summary.State = "Resolved";
				Save(summary);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, "An exception occured resolving the work item {0}", id);
			}
		}

		/// <summary>
		/// Changes the state of a work item to 'closed'.
		/// </summary>
		/// <param name="id">The id of the work item to change the state of.</param>
		public virtual void Close(int id)
		{
			QueryManager manager = new QueryManager();
			try
			{
				WorkItemSummary summary = manager.ItemById(id);
				summary.State = "Closed";
				Save(summary);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, "An exception occured closing the work item {0}", id);
			}
		}

		/// <summary>
		/// Deletes an attachment from a work item.
		/// </summary>
		/// <param name="id">The id of the work item to delete the attachment for.</param>
		/// <param name="url">The attachment URL, which should contain the TFS protocol at the start of the string.</param>
		/// <exception cref="SaveException">Thrown if a TFS server-based error occurs with the deletion.</exception>
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
					throw new SaveException(string.Format("Removing attachment {0} failed for id '{1}' - the attachment no longer exists.", url, id));
				}
			}
		}

		/// <summary>
		/// Creates a new <see cref="WorkItemSummary"/> object. This method should be overridden by implementing classes.
		/// </summary>
		/// <returns>A <see cref="WorkItemSummary"/> object, which should be specialized for particular work item type the manager handles.</returns>
		public abstract WorkItemSummary NewItem();

		/// <summary>
		/// Creates a new <see cref="WorkItem"/> from a <see cref="WorkItemSummary"/> instance, populating its core fields with 
		/// the data from the <see cref="WorkItemSummary"/> object.
		/// </summary>
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