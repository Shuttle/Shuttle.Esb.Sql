using System;

namespace Shuttle.Esb.Sql
{
	public class ScriptException : Exception
	{
		public ScriptException(string message)
			: base(message)
		{
		}
	}
}