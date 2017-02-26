using Castle.Windsor;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Sql.Tests
{
    public static class SqlFixture
    {
        public static ComponentContainer GetComponentContainer()
        {
            return GetComponentContainer(true);
        }

        public static ComponentContainer GetComponentContainer(bool registerIdempotenceService)
        {
            var container = new WindsorComponentContainer(new WindsorContainer());

            container.Register<IScriptProviderConfiguration, ScriptProviderConfiguration>();
			container.Register<IScriptProvider, ScriptProvider>();

			container.Register<ISqlConfiguration>(SqlSection.Configuration());
            container.Register<IDatabaseContextCache, ThreadStaticDatabaseContextCache>();
            container.Register<IDatabaseContextFactory, DatabaseContextFactory>();
            container.Register<IDbConnectionFactory, DbConnectionFactory>();
            container.Register<IDbCommandFactory, DbCommandFactory>();
            container.Register<IDatabaseGateway, DatabaseGateway>();

            if (registerIdempotenceService)
            {
                container.Register<IIdempotenceService, IdempotenceService>();
            }

            return new ComponentContainer(container, () => container);
        }
    }
}