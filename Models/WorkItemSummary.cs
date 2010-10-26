using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Spruce.Models
{
	public class WorkItemSummary
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public string Area { get; set; }
		public string Iteration { get; set; }
		public string State { get; set; }
		public string AssignedTo { get; set; }
		public string ResolvedBy { get; set; }
		public int? Priority { get; set; }
	}

	public static class HelperExtensions
	{
		public static string DropDownBoxFromList(this HtmlHelper helper,string name, IList<string> items,string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string item in items)
			{
				SelectListItem selectListItem = new SelectListItem() { Text = item,Value=item };
				if (item == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name,selectList).ToHtmlString();
		}
	}
}