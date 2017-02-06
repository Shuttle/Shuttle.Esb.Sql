using System.Reflection;
using Castle.Windsor;
using NUnit.Framework;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb.Sql.Idempotence;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Sql.Tests
{
	[TestFixture]
	public class SqlIdempotenceTest : IdempotenceFixture
	{
		[Test]
		[TestCase(false, false)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(true, true)]
		public void Should_be_able_to_perform_full_processing(bool isTransactionalEndpoint, bool enqueueUniqueMessages)
		{
            var container = new WindsorComponentContainer(new WindsorContainer());

		    var sqlServerConfiguration = SqlSection.Configuration();

            container.Register<ISqlConfiguration>(sqlServerConfiguration);
		    container.Register<IDatabaseContextCache, ThreadStaticDatabaseContextCache>();
		    container.Register<IDatabaseContextFactory, DatabaseContextFactory>();
            container.Register<IDbConnectionFactory, DbConnectionFactory>();
            container.Register<IDbCommandFactory, DbCommandFactory>();
            container.Register<IDatabaseGateway, DatabaseGateway>();
		    container.Register<IIdempotenceService, IdempotenceService>();

            container.Register<IScriptProvider>(new ScriptProvider(new ScriptProviderConfiguration
            {
                ResourceAssembly = Assembly.GetAssembly(typeof(SqlQueue)),
                ResourceNameFormat = "Shuttle.Esb.Sql.Scripts.System.Data.SqlClient.{ScriptName}.sql"
            }));

            TestIdempotenceProcessing(container, () => container, @"sql://shuttle/{0}", isTransactionalEndpoint, enqueueUniqueMessages);
		}
	}
}