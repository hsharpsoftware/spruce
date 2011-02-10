using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spruce.Core.Search;

namespace Spruce.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class SearchTest
	{
		public SearchTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestSearchParser()
		{
			string basicQuery = "project:spruce \"the title of the bug\" description:\"my description\" area:foo some more words OR -bug";

			SearchParser parser = new SearchParser();
			parser.ParseComplete += delegate(object sender,EventArgs e)
			{
				string wiql = parser.WiqlBuilder.ToString();
				Assert.AreEqual("Project = 'spruce' AND title = 'the title of the bug some more words' AND Description = 'my description' AND Area = 'foo' OR title != 'bug'",
								wiql);
			};
			parser.SearchFor(basicQuery);
		}
	}
}
