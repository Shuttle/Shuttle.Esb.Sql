using NUnit.Framework;

namespace Shuttle.Esb.Sql.Tests
{
	[TestFixture]
	public class SqlSectionTest : SqlSectionFixture
	{
		[Test]
		[TestCase("Sql.config")]
		[TestCase("Sql-Grouped.config")]
		public void Should_be_able_to_load_a_full_configuration(string file)
		{
			var section = GetSqlSection(file);

			Assert.IsNotNull(section);

			Assert.AreEqual("subscription-connection-string-name", section.SubscriptionManagerConnectionStringName);
			Assert.AreEqual(".\\custom-script-folder", section.ScriptFolder);
		}
	}
}