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
				return SpruceSection.Current.TfsServer;
			}
		}

		public static string DefaultProjectName
		{
			get
			{
				return SpruceSection.Current.DefaultProjectName;
			}
		}

		/// <summary>
		/// The directory for all attachments to be uploaded prior to being saved on TFS. Includes a trailing slash.
		/// </summary>
		public static string UploadDirectory
		{
			get
			{
				if (UserContext.IsWeb)
					return HttpContext.Current.Server.MapPath("~/App_Data/Attachments/");
				else
					return @"Attachments\";
			}
		}
	}
}