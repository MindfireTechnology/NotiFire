using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
