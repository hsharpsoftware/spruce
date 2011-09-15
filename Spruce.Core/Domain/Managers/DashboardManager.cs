using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections;

namespace Spruce.Core
{
	public class DashboardManager
	{
		public static DashboardSummary GetSummary()
		{
			BugManager bugManager = new BugManager();
			TaskManager taskManager = new TaskManager();

			IList<WorkItemSummary> allbugs = bugManager.ExecuteQuery();
			IList<WorkItemSummary> allTasks = taskManager.ExecuteQuery();

			DashboardSummary summary = new DashboardSummary();
			summary.RecentCheckins = RecentCheckins();

			bugManager.Active();
			
			summary.ActiveBugs = bugManager.ExecuteQuery().Count;
			summary.BugCount = allbugs.Count;
			summary.MyActiveBugCount = allbugs.Where(b => b.State == "Active").ToList().Count;
			summary.MyActiveBugs = allbugs.Where(b => b.State == "Active" && b.AssignedTo == UserContext.Current.Name)
				.OrderByDescending(b => b.CreatedDate).Take(5).ToList();

			taskManager.Active();
			summary.ActiveTasks = allTasks.Count;
			summary.TaskCount = allTasks.Count;
			summary.MyActiveTasks = allTasks.Where(b => b.State == "Active" && b.AssignedTo == UserContext.Current.Name)
				.OrderByDescending(b => b.CreatedDate).Take(5).ToList();

			return summary;
		}

		public static List<ChangesetSummary> RecentCheckins()
		{
			string path = UserContext.Current.CurrentProject.Path;
			var checkins = UserContext.Current.VersionControlServer.QueryHistory(
							path,
							VersionSpec.Latest,
							0,
							RecursionType.Full,
							null,
							new DateVersionSpec(DateTime.Now.AddDays(-7)), // version from
							VersionSpec.Latest, // version to
							int.MaxValue, // maxcount
							true,
							true,
							true);

			List<ChangesetSummary> list = new List<ChangesetSummary>();
			foreach (Changeset item in checkins)
			{
				ChangesetSummary summary = new ChangesetSummary()
				{
					Id = item.ChangesetId,
					Date = item.CreationDate,
					Message = item.Comment,
					User = item.Committer,
					Changes = null
				};

				foreach (Change change in item.Changes)
				{
					summary.Files.Add(change.Item.ArtifactUri.ToString());
				}

				list.Add(summary);
			}

			return list;
		}

		public static ChangesetSummary GetChangeSet(int id)
		{
			Changeset changeset = UserContext.Current.VersionControlServer.GetChangeset(id);
			ChangesetSummary summary = new ChangesetSummary()
			{
				Id = changeset.ChangesetId,
				Date = changeset.CreationDate,
				Message = changeset.Comment,
				User = changeset.Committer,
				Changes = changeset.Changes
			};

			return summary;
		}
	}
}
