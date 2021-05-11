namespace NotifyGenerator
{
	public static class SyntaxExtensions
	{
		public static string ShortTypeName(this string typeName)
		{
			if (typeName.Contains("."))
				return typeName.Substring(typeName.LastIndexOf('.')+1);

			return typeName;
		}
	}
}
