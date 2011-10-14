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
	/// <summary>
	/// An embedded RavenDB user settings provider. The database is stored inside the App_Data\UserSettings directory
	/// </summary>
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
			//_documentStore.UseEmbeddedHttpServer = true;
			//NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8081);
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
		/// Loads a <see cref="UserSettings"/> object for the user id provided from the RavenDB store.
		/// </summary>
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

		/// <summary>
		/// Persists the provided <see cref="UserSettings"/> object to the RavenDB store.
		/// </summary>
		/// <param name="settings"></param>
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
