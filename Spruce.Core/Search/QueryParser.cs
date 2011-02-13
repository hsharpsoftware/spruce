using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using System.IO;

namespace Spruce.Core.Search
{
	/// <summary>
	/// 
	/// </summary>
	public class QueryParser
	{
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<EventArgs> ParseComplete;

		/// <summary>
		/// Gets the grammar reader.
		/// </summary>
		public static CGTReader GrammarReader
		{
			get
			{
				if (_grammarReader == null)
					InitializeReader();

				return _grammarReader;
			}
		}

		/// <summary>
		/// Gets the wiql builder.
		/// </summary>
		public WiqlBuilder WiqlBuilder
		{
			get { return _wiqlBuilder; }
		}

		private static CGTReader _grammarReader;
		private WiqlBuilder _wiqlBuilder;
		private string _fieldName;
		private FieldComparison _comparisonType;

		public static void InitializeReader()
		{
			using (Stream stream = typeof(QueryParser).Assembly.GetManifestResourceStream("Spruce.Core.Search.Grammar.spruce.cgt"))
			{
				// Parse the compiled grammar file
				_grammarReader = new CGTReader(stream);
			}
		}

		public void SearchFor(string searchTerm)
		{
			LALRParser lalrParser = GrammarReader.CreateNewParser();
			_wiqlBuilder = new WiqlBuilder();
			_fieldName = "";
			_comparisonType = FieldComparison.Contains;

			lalrParser.OnTokenRead += new LALRParser.TokenReadHandler(lalrParser_OnTokenRead);
			lalrParser.OnParseError += new LALRParser.ParseErrorHandler(lalrParser_OnParseError);
			lalrParser.OnTokenError += new LALRParser.TokenErrorHandler(lalrParser_OnTokenError);
			lalrParser.Parse(searchTerm);
		}

		private void lalrParser_OnTokenError(LALRParser parser, TokenErrorEventArgs args)
		{
			OnParseComplete(EventArgs.Empty);
		}

		private void lalrParser_OnParseError(LALRParser parser, ParseErrorEventArgs args)
		{
			OnParseComplete(EventArgs.Empty);
		}

		private void lalrParser_OnTokenRead(LALRParser parser, TokenReadEventArgs e)
		{
			// Pass these names to the WIQLBuilder. It can translate them using its own lookup table
			// in to the appropriate field name.

			// Huge switch statement for each token type. Originally this was going to be transformed
			// into a type per symbol, however leaving the WiqlBuilder to deal with it seems a lot simpler.
			switch (e.Token.Symbol.Name)
			{
				//
				// Matched field identifiers
				//
				case "Project":
					_comparisonType = FieldComparison.Project;
					_fieldName = e.Token.Symbol.Name;
					break;

				case "AssignedTo":
				case "ResolvedBy":
					_comparisonType = FieldComparison.User;
					_fieldName = e.Token.Symbol.Name;
					break;

				case "CreatedOn":
				case "ResolvedOn":
					_comparisonType = FieldComparison.Date;
					_fieldName = e.Token.Symbol.Name;
					break;

				case "Description":
				case "State":
				case "Type":
				case "Area":
				case "Iteration":
					_comparisonType = FieldComparison.ExactMatch;
					_fieldName = e.Token.Symbol.Name;
					break;

				//
				// OR/NOT
				//
				case "Or":
					_fieldName = "";
					_wiqlBuilder.Or();
					break;

				case "Negate":
					_wiqlBuilder.Not();
					break;

				//
				// Strings
				//
				case "StringLiteral":
					_wiqlBuilder.AppendField(_fieldName, e.Token.Text.Replace("\"",""),FieldComparison.ExactMatch);
					_fieldName = "";
					_comparisonType = FieldComparison.Contains;
					break;

				case "AnyChar":
					_wiqlBuilder.AppendField(_fieldName, e.Token.Text,_comparisonType);
					_fieldName = "";
					_comparisonType = FieldComparison.Contains;
					break;

				// End of parsing
				case "(EOF)":
					OnParseComplete(EventArgs.Empty);
					break;

				default:
					// Exception?
					throw new NotImplementedException(string.Format("{0} is not supported",e.Token.Symbol.Name));
			}
		}

		protected void OnParseComplete(EventArgs e)
		{
			// TODO: make this thread safe?
			if (ParseComplete != null)
				ParseComplete(this, e);
		}
	}
}
