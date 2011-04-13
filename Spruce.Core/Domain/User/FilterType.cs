using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Spruce.Core
{
	/// <summary>
	/// This could potentially be a flags enum.
	/// </summary>
	public enum FilterType
	{
		All,
		Active,
		Resolved,
		Closed,
		[Description("Assigned to me")]
		AssignedToMe,
		Today,
		Yesterday,
		[Description("This week")]
		ThisWeek
	}
}
