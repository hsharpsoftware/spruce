using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Spruce.Core
{
	/// <summary>
	/// Provides an RSS feed using the data provided by the <see cref="Feed"/> property. This 
	/// <c>ActionResult</c> returns its data with the 'application/rss+xml' content type.
	/// </summary>
	public class RssActionResult : ActionResult
	{
		public SyndicationFeed Feed { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RssActionResult"/> class.
		/// </summary>
		public RssActionResult()
		{
		}

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.ContentType = "application/rss+xml";

			Rss20FeedFormatter formatter = new Rss20FeedFormatter(Feed);
			using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.Output))
			{
				formatter.WriteTo(writer);
			}
		}
	}
}
