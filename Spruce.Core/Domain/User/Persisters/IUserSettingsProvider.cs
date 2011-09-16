using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spruce.Core
{
	internal interface IUserSettingsProvider
	{
		UserSettings Load(Guid userId);
		void Save(UserSettings settings);
	}
}
