namespace Shuttle.Esb.Sql
{
	public interface ISqlConfiguration
	{
		string SubscriptionManagerProviderName { get; }
		string SubscriptionManagerConnectionString { get; }
		string IdempotenceServiceProviderName { get; }
		string IdempotenceServiceConnectionString { get; }
        bool IgnoreSubscribe { get; }
	}
}