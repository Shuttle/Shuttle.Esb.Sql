namespace Shuttle.Esb.Sql
{
	public class SqlConfiguration : ISqlConfiguration
	{
		public SqlConfiguration()
		{
		    IgnoreSubscribe = false;
		}

	    public string SubscriptionManagerProviderName { get; set; }
	    public string SubscriptionManagerConnectionString { get; set; }
	    public string IdempotenceServiceProviderName { get; set; }
	    public string IdempotenceServiceConnectionString { get; set; }
	    public bool IgnoreSubscribe { get; set; }
	}
}