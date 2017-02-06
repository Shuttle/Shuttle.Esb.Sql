using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Esb.Tests;

namespace Shuttle.Esb.Sql.Tests
{
    [TestFixture]
    public class SqlQueueTest : BasicQueueFixture
    {
        private QueueManager GetQueueManager()
        {
            var queueManager = new QueueManager(new DefaultUriResolver());

            var databaseContextCache = new ThreadStaticDatabaseContextCache();

            queueManager.RegisterQueueFactory(new SqlQueueFactory(new ScriptProvider(new ScriptProviderConfiguration
            {
                ResourceAssembly = typeof (SqlQueue).Assembly,
                ResourceNameFormat = "Shuttle.Esb.Sql.Scripts.System.Data.SqlClient.{ScriptName}.sql"
            }),
                new DatabaseContextFactory(new DbConnectionFactory(), new DbCommandFactory(), databaseContextCache),
                new DatabaseGateway()));

            return queueManager;
        }

        [Test]
        public void Should_be_able_to_get_message_again_when_not_acknowledged_before_queue_is_disposed()
        {
            using (var queueManager = GetQueueManager())
            {
                TestUnacknowledgedMessage(queueManager, "sql://shuttle/{0}");
            }
        }

        [Test]
        public void Should_be_able_to_perform_simple_enqueue_and_get_message()
        {
            using (var queueManager = GetQueueManager())
            {
                TestSimpleEnqueueAndGetMessage(queueManager, "sql://shuttle/{0}");
            }
        }

        [Test]
        public void Should_be_able_to_release_a_message()
        {
            using (var queueManager = GetQueueManager())
            {
                TestReleaseMessage(queueManager, "sql://shuttle/{0}");
            }
        }
    }
}