using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spruce.Core
{
	/// <summary>
	/// The exception that is thrown when TFS work item save error occurs.
	/// </summary>
	public class SaveException : Exception
	{
		public SaveException() { }
		public SaveException(string message) : base(message) { }
		public SaveException(string message, Exception inner) : base(message, inner) { }
		public SaveException(Exception inner,string message, params object[] args) : base(string.Format(message +inner,args), inner) { }
	}
}