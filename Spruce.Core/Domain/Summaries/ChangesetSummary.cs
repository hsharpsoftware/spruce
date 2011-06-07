﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Spruce.Core
{
	public class ChangesetSummary
	{
		public int Id { get; set; }
		public string Message { get; set; }
		public string User { get; set; }
		public DateTime Date { get; set; }
		public List<string> Files { get; set; }
		public Change[] Changes { get; set; }

		public ChangesetSummary()
		{
			Files = new List<string>();
		}
	}
}