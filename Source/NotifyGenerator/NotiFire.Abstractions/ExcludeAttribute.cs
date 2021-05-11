using System;

namespace NotiFire.Abstractions
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class ExcludeAttribute : Attribute
	{
		public ExcludeAttribute()
		{
		}
	}
}
