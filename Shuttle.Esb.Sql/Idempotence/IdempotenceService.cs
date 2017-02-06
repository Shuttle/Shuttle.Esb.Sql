using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Sql.Idempotence
{
	public class IdempotenceService : IIdempotenceService
	{
		private readonly IDatabaseContextFactory _databaseContextFactory;

		private readonly IDatabaseGateway _databaseGateway;
		private readonly string _idempotenceConnectionString;
		private readonly IScriptProvider _scriptProvider;

		public IdempotenceService(
			ISqlConfiguration configuration,
			IScriptProvider scriptProvider,
			IDatabaseContextFactory databaseContextFactory,
			IDatabaseGateway databaseGateway)
		{
			Guard.AgainstNull(configuration, "configuration");
			Guard.AgainstNull(scriptProvider, "scriptProvider");
			Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
			Guard.AgainstNull(databaseGateway, "databaseGateway");

			_scriptProvider = scriptProvider;
			_databaseContextFactory = databaseContextFactory;
			_databaseGateway = databaseGateway;

			_idempotenceConnectionString = configuration.IdempotenceServiceConnectionString;

			if (string.IsNullOrEmpty(_idempotenceConnectionString))
			{
				throw new ConfigurationErrorsException(string.Format(SqlResources.ConnectionStringEmpty,
					"IdempotenceService"));
			}
		}

		public ProcessingStatus ProcessingStatus(TransportMessage transportMessage)
		{
			try
			{
				using (
					var connection = _databaseContextFactory.Create(SqlConfiguration.ProviderName,
						_idempotenceConnectionString))
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						if (_databaseGateway.GetScalarUsing<int>(
							RawQuery.Create(
								_scriptProvider.Get(
									Script.IdempotenceHasCompleted))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1)
						{
							return Esb.ProcessingStatus.Ignore;
						}

						if (_databaseGateway.GetScalarUsing<int>(
							RawQuery.Create(
								_scriptProvider.Get(
									Script.IdempotenceIsProcessing))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1)
						{
							return Esb.ProcessingStatus.Ignore;
						}

						_databaseGateway.ExecuteUsing(
							RawQuery.Create(
								_scriptProvider.Get(
									Script.IdempotenceProcessing))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)
								.AddParameterValue(IdempotenceColumns.InboxWorkQueueUri,
									transportMessage.RecipientInboxWorkQueueUri)
								.AddParameterValue(IdempotenceColumns.AssignedThreadId,
									Thread.CurrentThread.ManagedThreadId));

						var messageHandled = _databaseGateway.GetScalarUsing<int>(
							RawQuery.Create(
								_scriptProvider.Get(
									Script.IdempotenceIsMessageHandled))
								.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId)) == 1;

						return messageHandled
							? Esb.ProcessingStatus.MessageHandled
							: Esb.ProcessingStatus.Assigned;
					}
					finally
					{
						transaction.CommitTransaction();
					}
				}
			}
			catch (SqlException ex)
			{
				var message = ex.Message.ToUpperInvariant();

				if (message.Contains("VIOLATION OF UNIQUE KEY CONSTRAINT") ||
				    message.Contains("CANNOT INSERT DUPLICATE KEY") || message.Contains("IGNORE MESSAGE PROCESSING"))
				{
					return Esb.ProcessingStatus.Ignore;
				}

				throw;
			}
		}

		public void ProcessingCompleted(TransportMessage transportMessage)
		{
			using (
				var connection = _databaseContextFactory.Create(SqlConfiguration.ProviderName,
					_idempotenceConnectionString))
			using (var transaction = connection.BeginTransaction())
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(
						_scriptProvider.Get(Script.IdempotenceComplete))
						.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId));

				transaction.CommitTransaction();
			}
		}

		public IEnumerable<Stream> GetDeferredMessages(TransportMessage transportMessage)
		{
			var result = new List<Stream>();

			using (_databaseContextFactory.Create(SqlConfiguration.ProviderName, _idempotenceConnectionString))
			{
				var rows = _databaseGateway.GetRowsUsing(
					RawQuery.Create(_scriptProvider.Get(Script.IdempotenceGetDeferredMessages))
						.AddParameterValue(IdempotenceColumns.MessageIdReceived, transportMessage.MessageId));

				foreach (var row in rows)
				{
					result.Add(new MemoryStream((byte[]) row["MessageBody"]));
				}
			}

			return result;
		}

		public void DeferredMessageSent(TransportMessage processingTransportMessage,
			TransportMessage deferredTransportMessage)
		{
			using (_databaseContextFactory.Create(SqlConfiguration.ProviderName, _idempotenceConnectionString))
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(_scriptProvider.Get(Script.IdempotenceDeferredMessageSent))
						.AddParameterValue(IdempotenceColumns.MessageId, deferredTransportMessage.MessageId));
			}
		}

		public void MessageHandled(TransportMessage transportMessage)
		{
			using (_databaseContextFactory.Create(SqlConfiguration.ProviderName, _idempotenceConnectionString))
			{
				_databaseGateway.ExecuteUsing(
					RawQuery.Create(_scriptProvider.Get(Script.IdempotenceMessageHandled))
						.AddParameterValue(IdempotenceColumns.MessageId, transportMessage.MessageId));
			}
		}

	    public IdempotenceService(IServiceBusConfiguration configuration)
	    {
            Guard.AgainstNull(configuration,"configuration");

            using (
                var connection = _databaseContextFactory.Create(SqlConfiguration.ProviderName,
                    _idempotenceConnectionString))
            using (var transaction = connection.BeginTransaction())
            {
                if (_databaseGateway.GetScalarUsing<int>(
                    RawQuery.Create(
                        _scriptProvider.Get(
                            Script.IdempotenceServiceExists))) != 1)
                {
                    throw new InvalidOperationException(SqlResources.IdempotenceDatabaseNotConfigured);
                }

                _databaseGateway.ExecuteUsing(
                    RawQuery.Create(
                        _scriptProvider.Get(
                            Script.IdempotenceInitialize))
                        .AddParameterValue(IdempotenceColumns.InboxWorkQueueUri,
                            configuration.Inbox.WorkQueue.Uri.ToString()));

                transaction.CommitTransaction();
            }
        }

        public bool AddDeferredMessage(TransportMessage processingTransportMessage, TransportMessage deferredTransportMessage, Stream deferredTransportMessageStream)
        {
            using (_databaseContextFactory.Create(SqlConfiguration.ProviderName, _idempotenceConnectionString))
            {
                _databaseGateway.ExecuteUsing(
                    RawQuery.Create(_scriptProvider.Get(Script.IdempotenceSendDeferredMessage))
                        .AddParameterValue(IdempotenceColumns.MessageId, deferredTransportMessage.MessageId)
                        .AddParameterValue(IdempotenceColumns.MessageIdReceived, processingTransportMessage.MessageId)
                        .AddParameterValue(IdempotenceColumns.MessageBody, deferredTransportMessageStream.ToBytes()));
            }

            return true;
        }
    }
}