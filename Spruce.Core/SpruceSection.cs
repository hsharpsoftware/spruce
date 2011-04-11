using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Spruce.Core
{
	public class SpruceSection : ConfigurationSection
	{
		private static SpruceSection _section;

		/// <summary>
		/// The current instance of the section. This is not a singleton but there is no requirement for this to be threadsafe.
		/// </summary>
		public static SpruceSection Current
		{
			get
			{
				if (_section == null)
					_section = ConfigurationManager.GetSection("spruce") as SpruceSection;

				return _section;
			}
		}

		/// <summary>
		/// The name of the TFS server to query.
		/// </summary>
		[ConfigurationProperty("tfsServer", IsRequired = true)]
		public string TfsServer
		{
			get { return (string)this["tfsServer"]; }
			set { this["tfsServer"] = value; }
		}

		/// <summary>
		/// The project name that is selected by default, before the user has changed this in their settings.
		/// </summary>
		[ConfigurationProperty("defaultProjectName", IsRequired = true)]
		public string DefaultProjectName
		{
			get { return (string)this["defaultProjectName"]; }
			set { this["defaultProjectName"] = value; }
		}

		// New additions (after a release candidate) should be marked IsRequired=false
	}
}
