using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Spruce.Core
{
	public class SpruceSettings
	{
		public static string TfsServer
		{
			get
			{
				return ConfigurationManager.AppSettings["TfsServer"];
			}
		}

		public static string DefaultProjectName
		{
			get
			{
				return ConfigurationManager.AppSettings["DefaultProjectName"];
			}
		}
	}
}