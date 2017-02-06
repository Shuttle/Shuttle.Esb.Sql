using System.Configuration;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Sql
{
	public class SqlSection : ConfigurationSection
	{
		[ConfigurationProperty("subscriptionManagerConnectionStringName", IsRequired = false,
			DefaultValue = "Subscription")]
		public string SubscriptionManagerConnectionStringName
		{
			get { return (string) this["subscriptionManagerConnectionStringName"]; }
		}

		[ConfigurationProperty("idempotenceServiceConnectionStringName", IsRequired = false,
			DefaultValue = "Idempotence")]
		public string IdempotenceServiceConnectionStringName
		{
			get { return (string) this["idempotenceServiceConnectionStringName"]; }
		}

		[ConfigurationProperty("scriptFolder", IsRequired = false, DefaultValue = null)]
		public string ScriptFolder
		{
			get { return (string) this["scriptFolder"]; }
		}

		[ConfigurationProperty("ignoreSubscribe", IsRequired = false, DefaultValue = false)]
		public bool IgnoreSubscribe
        {
			get { return (bool) this["ignoreSubscribe"]; }
		}

		public static SqlConfiguration Configuration()
		{
			var section = ConfigurationSectionProvider.Open<SqlSection>("shuttle", "sqlServer");
			var configuration = new SqlConfiguration();

			var subscriptionManagerConnectionStringName = "Subscription";
			var idempotenceServiceConnectionStringName = "Idempotence";

			if (section != null)
			{
				subscriptionManagerConnectionStringName = section.SubscriptionManagerConnectionStringName;
				idempotenceServiceConnectionStringName = section.IdempotenceServiceConnectionStringName;
				configuration.ScriptFolder = section.ScriptFolder;
			    configuration.IgnoreSubscribe = section.IgnoreSubscribe;
			}

			configuration.SubscriptionManagerConnectionString = GetConnectionString(subscriptionManagerConnectionStringName);
			configuration.IdempotenceServiceConnectionString = GetConnectionString(idempotenceServiceConnectionStringName);

			return configuration;
		}

		private static string GetConnectionString(string connectionStringName)
		{
			var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

			return settings == null ? string.Empty : settings.ConnectionString;
		}
	}
}