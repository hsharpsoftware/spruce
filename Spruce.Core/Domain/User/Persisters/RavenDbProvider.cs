using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Embedded;
using Raven.Client;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Raven.Http;

namespace Spruce.Core
{
	internal class RavenDbProvider : IUserSettingsProvider
	{
		private static EmbeddableDocumentStore _documentStore;

		static RavenDbProvider()
		{
			_documentStore = new EmbeddableDocumentStore()
			{
				DataDirectory = SpruceSettings.UserSettingsDirectory
			};

#if DEBUG
			_documentStore.UseEmbeddedHttpServer = true;
			NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8081);
#endif

			try
			{
				NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8081);
			}
			catch (Exception e)
			{
				Log.Warn(e, "RavenDb: NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8081) threw an exception");
			}

			_documentStore.Initialize();

			// Access RavenDb using http://localhost:8081. Make sure the XAP file from the lib/ravendb folder is in the Spruce.Site root.
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId">The userid provided by TFS for the current user</param>
		/// <returns></returns>
		public UserSettings Load(Guid userId)
		{
			using (IDocumentSession session = _documentStore.OpenSession())
			{
				UserSettings settings = session.Load<UserSettings>(userId.ToString());

				if (settings == null)
					settings = new UserSettings(userId);

				return settings;
			}
		}

		public void Save(UserSettings settings)
		{
			using (IDocumentSession session = _documentStore.OpenSession())
			{
				session.Store(settings);
				session.SaveChanges();
			}
		}
	}
}
