using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Spruce.Core
{
	public class Log
	{
		public static void Information(string message, params object[] args)
		{
			Write(ErrorType.Information, null, message, args);
		}

		public static void Information(Exception ex, string message, params object[] args)
		{
			Write(ErrorType.Information, ex, message, args);
		}

		public static void Warn(string message, params object[] args)
		{
			Write(ErrorType.Warning, null, message, args);
		}

		public static void Warn(Exception ex, string message, params object[] args)
		{
			Write(ErrorType.Warning, ex, message, args);
		}

		public static void Error(string message, params object[] args)
		{
			Write(ErrorType.Error, null, message, args);
		}

		public static void Error(Exception ex, string message, params object[] args)
		{
			Write(ErrorType.Error, ex, message, args);
		}

		public static void Write(ErrorType errorType, Exception ex, string message, params object[] args)
		{
			if (ex != null)
				message += "\n" + ex;

			// Trace should catch FormatException
			switch (errorType)
			{
				case ErrorType.Information:
					Trace.TraceInformation(message, args);
					break;

				case ErrorType.Error:
					Trace.TraceError(message, args);
					break;

				case ErrorType.Warning:
				default:
					Trace.TraceWarning(message, args);
					break;
			}
		}
	}

	public enum ErrorType
	{
		Information,
		Warning,
		Error
	}
}
