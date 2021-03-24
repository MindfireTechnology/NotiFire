using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotiFire
{
	[Generator]
	public class NotifyImplementationGenerator : ISourceGenerator
	{

		private static readonly DiagnosticDescriptor NestedClassWarning = new DiagnosticDescriptor(
			id: "NotiFireGEN001",
			title: "Nested Classes Are Not Supported",
			messageFormat: "Classes that are defined inside of another class (nested classes) are not supported",
			category: "NotiFireGenerator",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		protected const string NotifyAttribute = @"
using System;
namespace NotiFire
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
";

		protected const string ExcludeAttribute = @"
using System;
namespace NotiFire
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class ExcludeAttribute : Attribute
	{
		public ExcludeAttribute()
		{
		}
	}
}
";

		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context)
		{
			context.AddSource(nameof(NotifyAttribute), SourceText.From(NotifyAttribute, Encoding.UTF8));
			context.AddSource(nameof(ExcludeAttribute), SourceText.From(ExcludeAttribute, Encoding.UTF8));

			if (context.SyntaxReceiver is not SyntaxReceiver receiver)
				return;

			// create a new compilation that contains the attribute
			var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
			var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(NotifyAttribute, Encoding.UTF8), options));
			compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(ExcludeAttribute, Encoding.UTF8), options));

			// get the newly bound attribute
			var notifyAttributeSymbol = compilation.GetTypeByMetadataName("NotiFire.NotifyAttribute");
			var excludeAttributeSymbol = compilation.GetTypeByMetadataName("NotiFire.ExcludeAttribute");
			var notifyInterfaceSymbol = context.Compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged");

			foreach (var classSyntax in receiver.Classes)
			{
				var sourceModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
				var sourceSymbol = sourceModel.GetDeclaredSymbol(classSyntax);

				string notifyClassSource = BuildNotifyPartialClass(sourceSymbol, notifyAttributeSymbol, excludeAttributeSymbol, notifyInterfaceSymbol);

				string hintName = $"{sourceSymbol.ContainingNamespace}.{sourceSymbol.Name}.Notify.g.cs";
				context.AddSource(hintName, SourceText.From(notifyClassSource, Encoding.UTF8));
			}
		}

		/// <summary>
		/// Build the partial class which contains the INotifyPropertyChanged implementation
		/// </summary>
		/// <param name="classSymbol">Symbol for the class that's being added to</param>
		/// <param name="notifyAttributeSymbol">The notify attribute symbol</param>
		/// <param name="excludeAttributeSymbol">The exclude attribute symbol</param>
		/// <param name="notifyInterfaceSymbol">The symbol for the INotifyPropertyChanged interface</param>
		/// <returns></returns>
		protected string BuildNotifyPartialClass(
			INamedTypeSymbol classSymbol,
			INamedTypeSymbol notifyAttributeSymbol,
			INamedTypeSymbol excludeAttributeSymbol,
			ISymbol notifyInterfaceSymbol)
		{
			var classNotifyAttributes = classSymbol.GetAttributes().Where(a => a.AttributeClass.Equals(notifyAttributeSymbol, SymbolEqualityComparer.Default));

			var classMembers = classSymbol.GetMembers();
			var classFields = classMembers.Where(m => !m.IsImplicitlyDeclared && !m.IsStatic && m.Kind == SymbolKind.Field)
			.Cast<IFieldSymbol>();

			if (classNotifyAttributes.Any())
			{
				// Map all properties except those that are excluded
				classFields = classFields.Where(f =>
				{
					var attributes = f.GetAttributes().Where(a => a.AttributeClass.Equals(excludeAttributeSymbol, SymbolEqualityComparer.Default));
					return !attributes.Any();
				});
			}
			else
			{
				classFields = classFields.Where(f =>
				{
					var attributes = f.GetAttributes().Where(a => a.AttributeClass.Equals(notifyAttributeSymbol, SymbolEqualityComparer.Default));
					return attributes.Any();
				});
			}

			var notifySource = new StringBuilder($@"
using System;
using System.ComponentModel;

namespace {classSymbol.ContainingNamespace}
{{
	public partial class {classSymbol.Name}");

			if (notifyInterfaceSymbol == null)
			{
				var notifyInterface = classSymbol.AllInterfaces.FirstOrDefault(i => i.ContainingNamespace.Name == "System.ComponentModel"
											&& i.Name == "INotifyPropertyChanged");

				if (notifyInterface != null)
				{
					notifySource.AppendLine("\t{");
				}
				else
				{
					notifySource.AppendLine($@" : INotifyPropertyChanged
	{{
		public event PropertyChangedEventHandler PropertyChanged;");
				}
			}
			else if (!classSymbol.Interfaces.Contains(notifyInterfaceSymbol, SymbolEqualityComparer.Default))
			{
				notifySource.AppendLine($@" : INotifyPropertyChanged
	{{
		public event PropertyChangedEventHandler PropertyChanged;");
			}
			else
			{
				notifySource.AppendLine("\t{");
			}

	notifySource.AppendLine("\t\tprotected void NotifyOfChange(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");

			foreach (var field in classFields)
			{
				var notifyAttribute = field.GetAttributes().FirstOrDefault(a => a.AttributeClass.Equals(notifyAttributeSymbol, SymbolEqualityComparer.Default));

				string name;

				if (notifyAttribute != null)
				{
					name = notifyAttribute.NamedArguments.FirstOrDefault(na => na.Key == "PropertyName").Value.Value as string;
				}
				else if (field.Name.EndsWith("field", StringComparison.OrdinalIgnoreCase))
				{
					int index = field.Name.LastIndexOf("field", StringComparison.OrdinalIgnoreCase);
					name = field.Name.Substring(0, index);
				}
				else if (field.Name.StartsWith("_"))
				{
					name = field.Name.Substring(1);
				}
				else
				{
					name = $"{field.Name}Property";
				}

				notifySource.AppendLine($@"
		public {field.Type} {name}
		{{
			get => {field.Name};
			set {{ {field.Name} = value; NotifyOfChange(nameof({name})); }}
		}}");
				notifySource.AppendLine();
			}

			notifySource.AppendLine("\t}");
			notifySource.AppendLine("}");

			return notifySource.ToString();
		}
	}
}
