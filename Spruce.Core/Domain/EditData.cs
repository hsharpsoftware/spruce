using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spruce.Core
{
	public class EditData<TWorkItemSummary> where TWorkItemSummary : WorkItemSummary
	{
		public string PageTitle { get; set; }
		public IEnumerable<string> Users
		{
			get
			{
				return UserContext.Current.Users;
			}
		}

		public virtual TWorkItemSummary WorkItem { get; set; }
		public IEnumerable<string> Reasons { get; set; }
		public IEnumerable<string> States { get; set; }
	}
}
