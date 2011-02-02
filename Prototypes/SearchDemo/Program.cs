using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using com.calitha.goldparser;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			string compiledFile = "spruce.cgt";
			string searchTerm = "-cheddar OR -brie \"red leicester\" OR -\"chutney\" project:foo";
			searchTerm = "project:foobar resolved-by:chris description:\"yes said fred\" hello world";
			
			Console.WriteLine("[Searching for:]");
			Console.WriteLine(searchTerm);
			Console.WriteLine();

			using (FileStream fileStream = new FileStream(compiledFile, FileMode.Open))
			{
				// Parse the compiled grammar file
				CGTReader reader = new CGTReader(fileStream);
				LALRParser lalrParser = reader.CreateNewParser();

				// Register for events
				lalrParser.OnTokenRead += new LALRParser.TokenReadHandler(lalrParser_OnTokenRead);
				lalrParser.OnReduce += new LALRParser.ReduceHandler(lalrParser_OnReduce);
				lalrParser.OnShift += new LALRParser.ShiftHandler(lalrParser_OnShift);
				lalrParser.Parse(searchTerm);
			}

			Console.Read();
		}

		static void lalrParser_OnShift(LALRParser parser, ShiftEventArgs args)
		{
			
		}

		static void lalrParser_OnReduce(LALRParser parser, ReduceEventArgs args)
		{
			
		}

		static void lalrParser_OnTokenRead(LALRParser parser, TokenReadEventArgs args)
		{
			Console.WriteLine("{0,-30}:\t{2}",args.Token.Symbol,"",args.Token.Text);
		}
	}
}
