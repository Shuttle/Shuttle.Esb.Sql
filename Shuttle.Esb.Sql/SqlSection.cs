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

		[ConfigurationProperty("ignoreSubscribe", IsRequired = false, DefaultValue = false)]
		public bool IgnoreSubscribe
        {
			get { return (bool) this["ignoreSubscribe"]; }
		}

		public static SqlConfiguration Configuration()
		{
			var section = ConfigurationSectionProvider.Open<SqlSection>("shuttle", "sql");
			var configuration = new SqlConfiguration();

			var subscriptionManagerConnectionStringName = "Subscription";
			var idempotenceServiceConnectionStringName = "Idempotence";

			if (section != null)
			{
				subscriptionManagerConnectionStringName = section.SubscriptionManagerConnectionStringName;
				idempotenceServiceConnectionStringName = section.IdempotenceServiceConnectionStringName;
			    configuration.IgnoreSubscribe = section.IgnoreSubscribe;
			}

            configuration.SubscriptionManagerProviderName = GetProviderName(subscriptionManagerConnectionStringName);
            configuration.SubscriptionManagerConnectionString = GetConnectionString(subscriptionManagerConnectionStringName);
			configuration.IdempotenceServiceProviderName = GetProviderName(idempotenceServiceConnectionStringName);
			configuration.IdempotenceServiceConnectionString = GetConnectionString(idempotenceServiceConnectionStringName);

			return configuration;
		}

		private static string GetConnectionString(string connectionStringName)
		{
			var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

			return settings == null ? string.Empty : settings.ConnectionString;
		}

		private static string GetProviderName(string connectionStringName)
		{
			var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

			return settings == null ? string.Empty : settings.ProviderName;
		}
	}
}