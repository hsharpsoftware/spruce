using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core.Search
{
	public class WiqlBuilder
	{
		private StringBuilder _builder;
		private Stack<object> _stack;

		public WiqlBuilder()
		{
			_builder = new StringBuilder();
			_stack = new Stack<object>();
		}

		public void And()
		{
			// Check if an AND/OR isn't already in the stack
			if (_stack.Count > 0)
			{
				string last = _stack.Peek().ToString();
				if (last.StartsWith("[Field]")) // not a very intelligent way of doing it, but faster than using typeofs
				{
					_stack.Push("AND");
				}
			}
		}

		public void Or()
		{
			// Check if an AND/OR isn't already in the stack
			if (_stack.Count > 0)
			{
				string last = _stack.Peek().ToString();
				if (last.StartsWith("[Field]"))
				{
					_stack.Push("OR");
				}
			}
		}

		public void Not()
		{
			// Check if Field is in the stack.
			if (_stack.Count > 0)
			{
				Field field = _stack.Peek() as Field;
				if (field != null)
				{
					field.Not = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="field">If this is blank, then the title field is used as the default.</param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void AppendField(string fieldName,string value)
		{
			// If any field is added again, it needs to be AND'd
			// e.g. "richtextbox bugs spelling" should search the title
			// WHERE title="richtextbox" AND title="bugs" AND title="spelling"

			// It will need to be smart and look for an OR. With a Stack for the various
			// operators: OR, AND this is not difficult. Simply push an AND by default 
			// if the last token was a Field and not an operator.

			if (_stack.Count > 0)
			{
				Field field = _stack.Peek() as Field;
				if (field != null)
				{
					_stack.Push("AND");
				}
			}

			_stack.Push(new Field() { Name = fieldName, Value = value });
		}

		private class Field
		{
			public string Name { get; set; }
			public string Value { get; set; }
			public bool Not { get; set; }

			public override string ToString()
			{
				return string.Format("[Field] {0} {1} {2}", Name, (Not) ? "!=" : "=", Value);
			}
		}
	}
}
