using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using System.IO;

namespace Spruce.Core.Search
{
	public class SearchParser
	{
		public event EventHandler<EventArgs> ParseComplete;

		public static CGTReader GrammarReader
		{
			get
			{
				if (_grammarReader == null)
					InitializeReader();

				return _grammarReader;
			}
		}

		public WiqlBuilder WiqlBuilder
		{
			get { return _wiqlBuilder; }
		}

		private static CGTReader _grammarReader;
		private WiqlBuilder _wiqlBuilder;
		private string _fieldName;

		public static void InitializeReader()
		{
			using (Stream stream = typeof(SearchParser).Assembly.GetManifestResourceStream("Spruce.Core.Search.spruce.cgt"))
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

			lalrParser.OnTokenRead += new LALRParser.TokenReadHandler(lalrParser_OnTokenRead);
			lalrParser.Parse(searchTerm);
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
				case "Description":
				case "State":
				case "Type":
				case "Area":
				case "Iteration":
				case "CreatedBy":
				case "ResolvedBy":
				case "CreatedOn":
				case "ResolvedOn":
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
					_wiqlBuilder.AppendField(_fieldName, e.Token.Text.Replace("\"",""));
					_fieldName = "";
					break;

				case "AnyChar":
					_wiqlBuilder.AppendField(_fieldName, e.Token.Text);
					_fieldName = "";
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
