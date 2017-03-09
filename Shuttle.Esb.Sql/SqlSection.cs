using System;
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

			configuration.SubscriptionManagerProviderName = GetSettings(subscriptionManagerConnectionStringName).ProviderName;
			configuration.SubscriptionManagerConnectionString = GetSettings(subscriptionManagerConnectionStringName).ConnectionString;
			configuration.IdempotenceServiceProviderName = GetSettings(idempotenceServiceConnectionStringName).ProviderName;
			configuration.IdempotenceServiceConnectionString = GetSettings(idempotenceServiceConnectionStringName).ConnectionString;

			return configuration;
		}

		private static ConnectionStringSettings GetSettings(string connectionStringName)
		{
			var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

			if (settings == null)
			{
				throw new InvalidOperationException(string.Format(SqlResources.ConnectionStringMissing, connectionStringName));
			}

			return settings;
		}
	}
}