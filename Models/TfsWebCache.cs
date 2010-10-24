using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Spruce.Models
{
	/// <summary>
	/// A fairly dumb cache (collections and items have no links yet).
	/// </summary>
	public class TfsWebCache
	{
		public static void Add(string key,WorkItemCollection collection)
		{
			HttpContext.Current.Cache.Insert(key,collection);
		}

		public static void AddItem(int id, WorkItem item)
		{
			HttpContext.Current.Cache.Insert(id.ToString(), item);
		}

		public static bool Exists(string key)
		{
			return HttpContext.Current.Cache.Get(key) != null;
		}

		public static WorkItem RetrieveItem(int id)
		{
			return (WorkItem)HttpContext.Current.Cache.Get(id.ToString());
		}

		public static WorkItemCollection Retrieve(string key)
		{
			return (WorkItemCollection)HttpContext.Current.Cache.Get(key);
		}

		public static void Remove(string key)
		{
			HttpContext.Current.Cache.Remove(key);
		}
	}
}