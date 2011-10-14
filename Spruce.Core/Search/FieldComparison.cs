using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	/// <summary>
	/// Represents the type of comparison the field search is performing.
	/// </summary>
	public enum FieldComparison
	{
		/// <summary>
		/// An exact textual match
		/// </summary>
		ExactMatch,
		/// <summary>
		/// A username search
		/// </summary>
		User,
		/// <summary>
		/// The search is a date
		/// </summary>
		Date,
		/// <summary>
		/// The search is a project name
		/// </summary>
		Project,
		/// <summary>
		/// The search should look for anything that contains the text.
		/// </summary>
		Contains
	}
}
