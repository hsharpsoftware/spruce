using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spruce.Core
{
	public class UserSettings
	{
		public static Dictionary<Guid, UserSettings> _cache;

		public string ProjectName { get; set; }
		public string IterationName { get; set; }
		public string IterationPath { get; set; }
		public string AreaName { get; set; }
		public string AreaPath { get; set; }
		/// <summary>
		/// This should map to an action name in BugController
		/// </summary>
		public string BugView { get; set; }
		/// <summary>
		/// This should map to an action name in TaskController
		/// </summary>
		public string TaskView { get; set; }
		public FilterType FilterType { get; set; }

		static UserSettings()
		{
			_cache = new Dictionary<Guid, UserSettings>();
		}

		/// <summary>
		/// For now, the user settings database is just an in-memory store
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public static UserSettings Load(Guid userId)
		{
			if (_cache.ContainsKey(userId))
			{
				return _cache[userId];
			}
			else
			{
				UserSettings settings = new UserSettings();
				_cache.Add(userId, settings);

				return settings;
			}
		}

		public static void Save(Guid userId,UserSettings settings)
		{
			if (_cache.ContainsKey(userId))
			{
				_cache[userId] = settings;
			}
			else
			{
				_cache.Add(userId, settings);
			}
		}
	}
}