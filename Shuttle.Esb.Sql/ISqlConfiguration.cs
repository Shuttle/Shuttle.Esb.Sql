namespace Shuttle.Esb.Sql
{
	public interface ISqlConfiguration
	{
		string SubscriptionManagerConnectionString { get; }
		string IdempotenceServiceConnectionString { get; }
		string ScriptFolder { get; }
        bool IgnoreSubscribe { get; }
	}
}