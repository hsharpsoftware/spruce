﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Spruce.Models
{
	public class Settings
	{
		public static string TfsServer
		{
			get
			{
				return ConfigurationManager.AppSettings["TfsServer"];
			}
		}

		public static string TfsProject
		{
			get
			{
				return ConfigurationManager.AppSettings["TfsProject"];
			}
		}

		public static string DescriptionField
		{
			get
			{
				return ConfigurationManager.AppSettings["DescriptionField"];
			}
		}
	}
}