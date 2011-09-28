using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Globalization;

namespace Spruce.Core
{
	public class ExtendedRazorViewEngine : RazorViewEngine
	{
		public ExtendedRazorViewEngine() : base() { }
		public ExtendedRazorViewEngine(IViewPageActivator viewPageActivator) : base(viewPageActivator) { }

		public void AddViewLocationFormat(string paths)
		{
			List<string> existingPaths = new List<string>(ViewLocationFormats);
			existingPaths.Add(paths);

			ViewLocationFormats = existingPaths.ToArray();
		}

		public void AddPartialViewLocationFormat(string paths)
		{
			List<string> existingPaths = new List<string>(PartialViewLocationFormats);
			existingPaths.Add(paths);

			PartialViewLocationFormats = existingPaths.ToArray();
		}
	}
}
