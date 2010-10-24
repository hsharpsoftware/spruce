using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
namespace Spruce.Models
{
	public interface IWorkItemManager
	{
		WorkItemCollection AllActiveBugs();
		WorkItemCollection AllBugs();
		WorkItemCollection AllClosedBugs();
		WorkItemCollection AllItems();
		WorkItemCollection AllTasks();
		WorkItem ItemById(int id);
		ProjectCollection Projects();
	}
}
