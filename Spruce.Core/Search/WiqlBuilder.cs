using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Spruce.Core.Search
{
	/// <summary>
	/// Generates a WIQL query.
	/// </summary>
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
		/// Initializes a new instance of the <see cref="WiqlBuilder"/> class.
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
		public void AppendField(string fieldName,string value,FieldComparison fieldType)
		{			
			if (string.IsNullOrEmpty(fieldName))
				fieldName = "Title";

			Field field = null;
			string key = fieldName + _nextFieldIsNot;

			if (_queryStack.Count > 0)
			{
				field = _queryStack.Peek() as Field;
				if (field != null)
				{
					if (!_fieldLookup.ContainsKey(key) && field.ToString() != "")
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
				switch (fieldType)
				{
					case FieldComparison.User:
						field = new UserField();
						break;

					case FieldComparison.Date:
						field = new DateField();
						break;

					case FieldComparison.Project:
						field = new ProjectField();
						break;

					case FieldComparison.Contains:
						field = new ContainsField();
						break;

					case FieldComparison.ExactMatch:
					default:
						field = new Field();
						break;
				}

				field.ColumnName = ToColumnName(fieldName);
				field.ParameterName = fieldName;
				field.Value = value;
				field.IsNot = _nextFieldIsNot;

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
		public string BuildQuery(Dictionary<string, object> parameters)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("SELECT * FROM Issue WHERE ");

			if (parameters == null)
				parameters = new Dictionary<string, object>();

			IList<object> stackList = _queryStack.ToList();
			stackList = stackList.Reverse().ToList(); // turn our LIFO into a FIFO to preserve the query order
			Dictionary<string, int> nameClashLookup = new Dictionary<string, int>();

			for (int i = 0; i < stackList.Count; i++)
			{
				Field field = stackList[i] as Field;
				if (field != null)
				{
					// Check for more than one field check (for OR queries)
					if (parameters.ContainsKey(field.ParameterName))
					{
						int uniqueId = 1;
						if (nameClashLookup.ContainsKey(field.ParameterName))
							uniqueId = ++nameClashLookup[field.ParameterName];
						else
							nameClashLookup.Add(field.ParameterName,uniqueId);

						field.ParameterName += uniqueId;
					}
						
					parameters.Add(field.ParameterName, field.Value);
					builder.Append(field);
				}
				else
				{
					// This avoids AND/OR being erronously added to the start or end of the query.
					// TODO: The And() and Or() methods should really deal with this
					if (i > 0 && i < stackList.Count -1)
						builder.AppendFormat(" {0} ", stackList[i].ToString());
				}
			}

			return builder.ToString();
		}

		private string ToColumnName(string name)
		{
			switch (name.ToLower())
			{
				case "project":
					return "[System.TeamProject]";

				case "description":
					return "[System.Description]";

				case "type":
					return "[Work Item Type]";

				case "state":
					return "State";

				case "iteration":
					return "[System.IterationPath]";

				case "area":
					return "[System.AreaPath]";

				case "assignedto":
					return "[Assigned To]";

				case "resolvedby":
					return "[Resolved By]";

				case "resolvedon":
					return "[Resolved Date]";

				case "createdon":
					return "[Created Date]";

				case "title":
				case "":
				default:
					return "[System.Title]";
			}
		}
	}
}
