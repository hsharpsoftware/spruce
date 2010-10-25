using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spruce.Models
{
	public class DisplaySettings
	{
		public IList<DisplayItem> DisplayItems { get; set; }

		/// <summary>
		/// If blank, WorkItem.Description is used.
		/// </summary>
		public string DescriptionField { get; set; }
	}
}