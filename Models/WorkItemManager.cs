using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;
using System.Net;
using System.Security.Principal;

namespace Spruce.Models
{
	public class WorkItemManager
	{
		private static TfsTeamProjectCollection _collection;
		private static string _projectName;
		private static WorkItemStore store;

		public static void Configure(IIdentity identity)
		{
			_projectName = "Spruce Test";

			_collection = new TfsTeamProjectCollection(new Uri(Settings.TfsServer), CredentialCache.DefaultCredentials);
			_collection.Authenticate();

			store = new WorkItemStore(_collection);
		}

		public static ProjectCollection Projects()
		{
			WorkItemStore store = new WorkItemStore(_collection);
			return store.Projects;
		}

		public static WorkItem ItemById(int id)
		{
			//if (TfsWebCache.Exists(id.ToString()))
			//    return TfsWebCache.RetrieveItem(id);

			WorkItem item = store.GetWorkItem(id);
			TfsWebCache.AddItem(id, item);

			return item;
		}

		public static WorkItemCollection AllBugs()
		{
			const string key = "allbugs";

			//if (TfsWebCache.Exists(key))
			//	return TfsWebCache.Retrieve(key);

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' ORDER BY Id DESC", _projectName.Replace("'", "''"));

			WorkItemCollection collection = store.Query(query);
			TfsWebCache.Add(key, collection);

			return collection;
		}

		public static WorkItemCollection AllItems()
		{
			const string key = "allitems";

			//if (TfsWebCache.Exists(key))
			//    return TfsWebCache.Retrieve(key);

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' ORDER BY Id DESC", _projectName.Replace("'", "''"));

			WorkItemCollection collection = store.Query(query);
			TfsWebCache.Add(key, collection);

			return collection;
		}

		public static WorkItemCollection AllClosedBugs()
		{	
			const string key = "allclosed";

			//if (TfsWebCache.Exists(key))
			//    return TfsWebCache.Retrieve(key);

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Closed' ORDER BY Id DESC", _projectName.Replace("'", "''"));

			WorkItemCollection collection = store.Query(query);
			TfsWebCache.Add(key, collection);

			return collection;
		}

		public static WorkItemCollection AllActiveBugs()
		{
			const string key = "allactive";

			//if (TfsWebCache.Exists(key))
			//    return TfsWebCache.Retrieve(key);

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Bug' AND State='Active' ORDER BY Id DESC", _projectName.Replace("'", "''"));

			WorkItemCollection collection = store.Query(query);
			TfsWebCache.Add(key, collection);

			return collection;
		}

		public static WorkItemCollection AllTasks()
		{		
			const string key = "alltasks";

			//if (TfsWebCache.Exists(key))
			//    return TfsWebCache.Retrieve(key);

			string query = string.Format("SELECT ID, Title from Issue WHERE System.TeamProject = '{0}' AND [Work Item Type]='Task' ORDER BY Id", _projectName.Replace("'", "''"));

			WorkItemCollection collection = store.Query(query);
			TfsWebCache.Add(key, collection);

			return collection;
		}
	}
}