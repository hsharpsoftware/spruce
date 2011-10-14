using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Globalization;

namespace Spruce.Core
{
	/// <summary>
	/// Extends the Razor view engine to allow extra search paths for partial and full views.
	/// </summary>
	public class ExtendedRazorViewEngine : RazorViewEngine
	{
		public ExtendedRazorViewEngine() : base() { }
		public ExtendedRazorViewEngine(IViewPageActivator viewPageActivator) : base(viewPageActivator) { }

		/// <summary>
		/// Adds a view location directory with the provided path.
		/// </summary>
		public void AddViewLocationFormat(string paths)
		{
			List<string> existingPaths = new List<string>(ViewLocationFormats);
			existingPaths.Add(paths);

			ViewLocationFormats = existingPaths.ToArray();
		}

		/// <summary>
		/// Adds a partial view location directory with the provided path.
		/// </summary>
		public void AddPartialViewLocationFormat(string paths)
		{
			List<string> existingPaths = new List<string>(PartialViewLocationFormats);
			existingPaths.Add(paths);

			PartialViewLocationFormats = existingPaths.ToArray();
		}
	}
}
