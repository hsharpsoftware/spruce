using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spruce.Core.Search;
using Spruce.Core;

namespace Spruce.Tests
{
	/// <summary>
	/// These tests are primarily black-box tests for the parser and WIQL builder.
	/// </summary>
	[TestClass]
	public class SearchTests
	{
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

		[TestMethod]
		public void TestSearchParser_WithLiterals()
		{
			string searchQuery = "project:spruce \"the title of the bug\" description:\"my description\" area:foo some more words OR -bug";
			string expectedWiql = "SELECT * FROM Issue WHERE [System.TeamProject] = @Project " +
				"AND [System.Title] = @Title "+
				"AND [System.Description] = @Description "+
				"AND [System.AreaPath] = @Area "+
				"OR [System.Title] NOT CONTAINS @Title1";

			QueryParser parser = new QueryParser();
			parser.ParseComplete += delegate(object sender,EventArgs e)
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				string actual = parser.WiqlBuilder.BuildQuery(parameters);

				Assert.IsTrue(parameters["Project"].ToString() == "spruce");
				Assert.IsTrue(parameters["Title"].ToString() == "the title of the bug some more words");
				Assert.IsTrue(parameters["Description"].ToString() == "my description");
				Assert.IsTrue(parameters["Area"].ToString() == "foo");
				Assert.IsTrue(parameters["Title1"].ToString() == "bug");
				Assert.AreEqual(expectedWiql, actual);
			};
			parser.SearchFor(searchQuery);
		}

		[TestMethod]
		public void TestSearchParser_WithDates()
		{
			SpruceContext.IsWeb = false;
			string searchQuery = "project:sprucetest created-on:yesterday resolved-on:thisweek";
			string expectedWiql = "SELECT * FROM Issue WHERE [System.TeamProject] = @Project " +
				"AND [Created Date] > @CreatedOn " +
				"AND [Resolved Date] > @ResolvedOn";

			QueryParser parser = new QueryParser();
			IList<WorkItemSummary> summaries = null;
			parser.ParseComplete += delegate(object sender, EventArgs e)
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				string actual = parser.WiqlBuilder.BuildQuery(parameters);

				Assert.IsTrue(parameters["Project"].ToString() == "sprucetest");
				Assert.IsTrue(parameters["CreatedOn"].ToString() == DateTime.Now.Yesterday().ToString());
				Assert.IsTrue(parameters["ResolvedOn"].ToString() == DateTime.Now.StartOfWeek().ToString());
				Assert.AreEqual(expectedWiql, actual);

				summaries = WorkItemManager.ExecuteWiqlQuery(actual, parameters, false);
			};
			parser.SearchFor(searchQuery);

			Assert.IsNotNull(summaries);
		}

		[TestMethod]
		public void TestSearchParser_WithUsers()
		{
			SpruceContext.IsWeb = false;
			string searchQuery = "project:sprucetest assigned-to:chris API test";
			string expectedWiql = "SELECT * FROM Issue WHERE [System.TeamProject] = @Project " +
				"AND [Assigned To] IN (@AssignedTo) " +
				"AND [System.Title] CONTAINS @Title";

			QueryParser parser = new QueryParser();
			IList<WorkItemSummary> summaries = null;
			parser.ParseComplete += delegate(object sender, EventArgs e)
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				string actual = parser.WiqlBuilder.BuildQuery(parameters);

				Assert.IsTrue(parameters["Project"].ToString() == "sprucetest");
				Assert.IsTrue(parameters["AssignedTo"].ToString() == "chris");
				Assert.IsTrue(parameters["Title"].ToString() == "API test");
				Assert.AreEqual(expectedWiql, actual);

				summaries = WorkItemManager.ExecuteWiqlQuery(actual, parameters, false);
			};
			parser.SearchFor(searchQuery);

			Assert.IsNotNull(summaries);
		}

		[TestMethod]
		public void TestExecuteWiql()
		{
			SpruceContext.IsWeb = false;
			
			string query = "SELECT * FROM Issue WHERE [System.AssignedTo] IN ('@Users')";
			Dictionary<string,object> parameters = new Dictionary<string,object>();
			parameters.Add("Users", "chris,tfsuser");
			
			IList<WorkItemSummary> summaries = WorkItemManager.ExecuteWiqlQuery(query, parameters, false);
			Assert.IsNotNull(summaries);
		}

		[TestMethod]
		public void TestStarProject()
		{
			string searchQuery = "project:* \"the title of the bug\"";
			string expectedWiql = "SELECT * FROM Issue WHERE [System.Title] = @Title";

			QueryParser parser = new QueryParser();
			parser.ParseComplete += delegate(object sender, EventArgs e)
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				string actual = parser.WiqlBuilder.BuildQuery(parameters);

				Assert.IsTrue(parameters["Title"].ToString() == "the title of the bug");
				Assert.AreEqual(expectedWiql, actual);
			};
			parser.SearchFor(searchQuery);
		}

		[TestMethod]
		public void TestSearchManager()
		{
			SpruceContext.IsWeb = false;
			string basicQuery = "\"API test\"";

			SearchManager manager = new SearchManager();
			IList<WorkItemSummary> summaries = manager.Search(basicQuery);		
		}


		[TestMethod]
		public void TestNumberOnlySearch()
		{
			// TODO: add a base class that adds/tears down for this ID
			SpruceContext.IsWeb = false;
			string basicQuery = "39";

			SearchManager manager = new SearchManager();
			IList<WorkItemSummary> summaries = manager.Search(basicQuery);

			Assert.IsTrue(summaries.Count == 1);
		}

		[TestMethod]
		public void TestField()
		{
			// I won't test every type as that's just replicating the production code, a small sample is fine.
			Field field = new Field();
			field.Value = "one two three";
			field.ParameterName = "Literal";
			field.ColumnName = "Title";
			Assert.AreEqual(field.ToString(), "Title = @Literal");
			Assert.AreEqual(field.Value, "one two three");

			field = new Field();
			field.ParameterName = "Literal";
			field.IsNot = true;
			field.ColumnName = "Title";
			Assert.AreEqual(field.ToString(), "Title != @Literal");
		}

		[TestMethod]
		public void TestContainsField()
		{
			// I won't test every type as that's just replicating the production code, a small sample is fine.
			ContainsField field = new ContainsField();
			field.Value = "one two three";
			field.ParameterName = "Keywords";
			field.ColumnName = "Title";
			Assert.AreEqual(field.ToString(), "Title CONTAINS @Keywords");
			Assert.AreEqual(field.Value, "one two three");

			field = new ContainsField();
			field.ParameterName = "Keywords";
			field.IsNot = true;
			field.ColumnName = "Title";
			Assert.AreEqual(field.ToString(), "Title NOT CONTAINS @Keywords");
		}

		[TestMethod]
		public void TestUserField()
		{
			UserField field = new UserField();
			field.Value = "jon,chris,adam,luke";
			field.ColumnName = "AssignedTo";
			field.ParameterName = "Users";
			Assert.AreEqual(field.Value, "'jon','chris','adam','luke'");
			Assert.AreEqual(field.ToString(), "AssignedTo IN (@Users)");

			field = new UserField();
			field.ColumnName = "AssignedTo";
			field.ParameterName = "Users";
			field.IsNot = true;
			Assert.AreEqual(field.ToString(), "AssignedTo NOT IN (@Users)");
		}

		[TestMethod]
		public void TestProjectField()
		{
			ProjectField field = new ProjectField();
			field.Value = "*";
			Assert.AreEqual(field.ToString(), "");

			field = new ProjectField();
			field.Value = "spruce";
			Assert.AreEqual(field.Value, "spruce");

			field = new ProjectField();
			field.IsNot = true;
			field.ColumnName = "Project";
			field.ParameterName = "Project";
			Assert.AreEqual(field.ToString(), "Project != @Project");
		}

		[TestMethod]
		public void TestDateField()
		{
			// I won't test every type as that's just replicating the production code, a small sample is fine.
			DateField field = new DateField();
			field.Value = "today";
			field.ParameterName = "Date";
			Assert.AreEqual(field.ToString(), " > @Date");
			Assert.AreEqual(field.Value, DateTime.Today);

			field = new DateField();
			field.Value = "yesterday";
			Assert.AreEqual(field.Value, DateTime.Today.AddDays(-1));

			field = new DateField();
			field.Value = "today";
			field.IsNot = true;
			field.ParameterName = "Date";
			Assert.AreEqual(field.ToString(), " <> @Date");
		}
	}
}
