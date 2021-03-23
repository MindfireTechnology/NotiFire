using System;
namespace NotifyGenerator
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class MapClassToAttribute : Attribute
	{
		public Type MappingType { get; }
		public bool Reverse { get; set; }
		public string MethodName { get; set; }

		public MapClassToAttribute(Type mappingType)
		{
			MappingType = mappingType;
		}
	}
}