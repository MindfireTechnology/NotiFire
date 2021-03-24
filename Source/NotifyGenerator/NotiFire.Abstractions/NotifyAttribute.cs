using System;

namespace NotiFire.Abstractions
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class NotifyAttribute : Attribute
	{
		public string PropertyName { get; set; }

		public NotifyAttribute()
		{
		}
	}
}
