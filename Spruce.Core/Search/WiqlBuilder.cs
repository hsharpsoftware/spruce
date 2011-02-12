using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Spruce.Core.Search
{
	public class WiqlBuilder
	{
		/// <summary>
		/// The field/operators are stored in LIFO list, as the order of the search query is parsed in operator/field order. So when
		/// we peek the stack, we want the last item added, and not a FIFO peek (Queue) where the item at the beginning is given.
		/// </summary>
		private Stack<object> _queryStack;
		
		/// <summary>
		/// NOT operators are given before the field, so keep track of them for the next Field.
		/// </summary>
		private bool _nextFieldIsNot;

		/// <summary>
		/// This allows repeated field searches (which is usually another keyword)
		/// to be combined into one string, instead of lots of title="x" AND title="y"
		/// </summary>
		private IDictionary<string, Field> _fieldLookup;

		/// <summary>
		/// 
		/// </summary>
		public WiqlBuilder()
		{
			_queryStack = new Stack<object>();
			_fieldLookup = new Dictionary<string, Field>();
		}

		/// <summary>
		/// 
		/// </summary>
		public void And()
		{
			if (_queryStack.Count > 0)
			{
				// Only add an AND if there's a field previously in the stack
				Field field = _queryStack.Peek() as Field;
				if (field != null)
				{
					_queryStack.Push("AND");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Or()
		{
			if (_queryStack.Count > 0)
			{
				// Only add an OR if there's a field previously in the stack
				Field field = _queryStack.Peek() as Field;
				if (field != null)
				{
					_queryStack.Push("OR");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Not()
		{
			// Check if Field is in the stack.
			if (_queryStack.Count > 0)
			{
				_nextFieldIsNot = true;
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
			if (string.IsNullOrEmpty(fieldName))
				fieldName = "title";

			Field field;
			string key = fieldName + _nextFieldIsNot;

			if (_queryStack.Count > 0)
			{
				field = _queryStack.Peek() as Field;
				if (field != null)
				{
					if (!_fieldLookup.ContainsKey(key))
					{
						_queryStack.Push("AND");
					}
				}
			}

			if (_fieldLookup.ContainsKey(key))
			{
				// Combine repeated field definitions into a single = (instead of repeating them with ANDs)
				_fieldLookup[key].Value += " " + value;
			}
			else
			{

				field = new Field()
				{
					Name = fieldName,
					Value = value,
					Not = _nextFieldIsNot
				};

				_queryStack.Push(field);
				_fieldLookup.Add(key, field);
			}

			// reset for future fields
			_nextFieldIsNot = false; 
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters">This dictionary is filled with name/value pairs for each field's name and value,
		/// for the WIQL query. Can be null.</param>
		/// <returns></returns>
		public string BuildQuery(Dictionary<string, string> parameters)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("SELECT * FROM Issue WHERE ");

			if (parameters == null)
				parameters = new Dictionary<string, string>();

			IList<object> stackList = _queryStack.ToList();
			stackList = stackList.Reverse().ToList(); // turn our LIFO into a FIFO to preserve the query order

			for (int i = 0; i < stackList.Count; i++)
			{
				Field field = stackList[i] as Field;
				if (field != null)
				{
					if (parameters.ContainsKey(field.Name))
						parameters[field.Name] = field.Value;
					else
						parameters.Add(field.Name, field.Value);

					builder.Append(field);
				}
				else
				{
					// Avoid adding AND/OR at the end of the query
					if (i < stackList.Count -1)
						builder.AppendFormat(" {0} ", stackList[i].ToString());
				}
			}

			return builder.ToString();
		}

		private class Field
		{
			public string Name { get; set; }
			public string Value { get; set; }
			public bool Not { get; set; }

			public override string ToString()
			{
				// TODO: replace with special names like System.ProjectName (in another class).
				// Check if project:* and then simply don't add.
				return string.Format("{0} {1} @{0}", Name, (Not) ? "!=" : "=");
			}
		}
	}
}
