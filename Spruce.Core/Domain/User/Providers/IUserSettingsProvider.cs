using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	/// <summary>
	/// Defines a storage mechanism that is responsible for loading and saving user settings.
	/// </summary>
	internal interface IUserSettingsProvider
	{
		/// <summary>
		/// Loads a <see cref="UserSettings"/> object for the user id provided. If the user
		/// does not have any settings yet, this method should return a default <see cref="UserSettings"/> object.
		/// </summary>
		/// <param name="userId">The unique ID of the user, which is based on the Team Foundation Server user Id.</param>
		UserSettings Load(Guid userId);

		/// <summary>
		/// Persists the provided <see cref="UserSettings"/> object to the underlying datastore.
		/// </summary>
		void Save(UserSettings settings);
	}
}
