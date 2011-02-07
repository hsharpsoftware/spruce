using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	public class WiqlBuilder
	{
		private StringBuilder _builder;

		public WiqlBuilder()
		{
			_builder = new StringBuilder();
		}

		public WiqlBuilder And()
		{
			return this;
		}

		public WiqlBuilder Or()
		{
			return this;
		}

		public WiqlBuilder FieldEquals(string field,string value)
		{
			return this;
		}
	}
}
