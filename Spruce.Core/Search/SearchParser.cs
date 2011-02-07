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
		// Passes a WIQL string
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

		private static CGTReader _grammarReader;
		private WiqlBuilder _wiqlBuilder;

		public static void InitializeReader()
		{
			using (Stream stream = typeof(SearchParser).Assembly.GetManifestResourceStream("Spruce.Core.Search.Spruce.cgt"))
			{
				// Parse the compiled grammar file
				_grammarReader = new CGTReader(stream);
			}
		}

		public void SearchFor(string searchTerm)
		{
			LALRParser lalrParser = GrammarReader.CreateNewParser();
			_wiqlBuilder = new WiqlBuilder();

			// Register for events
			lalrParser.OnTokenRead += new LALRParser.TokenReadHandler(lalrParser_OnTokenRead);
			lalrParser.Parse(searchTerm);
		}

		private void lalrParser_OnTokenRead(LALRParser parser, TokenReadEventArgs args)
		{
			// Huge switch statement

			// Pop a Term onto the queue

			// Peek top queue item

			// 
		}
	}
}
