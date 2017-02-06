using System;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Sql.Tests
{
	public class SqlSectionFixture
	{
		protected SqlSection GetSqlSection(string file)
		{
			return ConfigurationSectionProvider.OpenFile<SqlSection>("shuttle", "sqlServer", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@".\SqlSection\files\{0}", file)));
		}
	}
}