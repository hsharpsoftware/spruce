using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spruce.Core
{
	/// <summary>
	/// Contains data that is passed to and from views/controllers that are responsible 
	/// for editing work items.
	/// </summary>
	/// <typeparam name="TWorkItemSummary">A WorkItemSummary type that the data is for.</typeparam>
	public class EditData<TWorkItemSummary> where TWorkItemSummary : WorkItemSummary
	{
		public string PageTitle { get; set; }
		public string FromUrl { get; set; }
		public virtual TWorkItemSummary WorkItem { get; set; }
		public IEnumerable<string> Reasons { get; set; }
		public IEnumerable<string> States { get; set; }

		public IEnumerable<string> Users
		{
			get
			{
				return UserContext.Current.Users;
			}
		}
	}
}
