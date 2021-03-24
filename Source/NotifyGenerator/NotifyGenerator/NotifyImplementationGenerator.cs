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
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context)
		{

			if (context.SyntaxReceiver is not SyntaxReceiver receiver)
				return;

			var compilation = context.Compilation;

			// get the needed attributes
			var notifyAttributeSymbol = compilation.GetTypeByMetadataName("NotiFire.Abstractions.NotifyAttribute");
			var excludeAttributeSymbol = compilation.GetTypeByMetadataName("NotiFire.Abstractions.ExcludeAttribute");
			var notifyInterfaceSymbol = context.Compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged");

			foreach (var classSyntax in receiver.Classes)
			{
				var sourceModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
				var sourceSymbol = sourceModel.GetDeclaredSymbol(classSyntax);

				if (!sourceSymbol.GetAttributes().Any(a => a.AttributeClass.Equals(notifyAttributeSymbol, SymbolEqualityComparer.Default))
					&& !sourceSymbol.GetMembers().Any(m => m.Kind == SymbolKind.Field
														&& !m.IsImplicitlyDeclared && !m.IsStatic))
				{
					continue;
				}

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

			var classMembers = classSymbol.GetMembers().Where(m => !m.IsImplicitlyDeclared && !m.IsStatic);
			var classFields = classMembers.Where(m => m.Kind == SymbolKind.Field && !m.IsAbstract && m.IsDefinition)
			.Cast<IFieldSymbol>()
			.Where(f => !f.IsConst && !f.IsReadOnly && !f.IsVolatile);

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

			var method_t = classMembers.FirstOrDefault(m => m.Name == "NotifyOfChange");

			if (method_t != null)
			{
				bool isMethod = method_t.Kind == SymbolKind.Method;
				bool arity = (method_t as IMethodSymbol).Arity == 1;
				var parameters = (method_t as IMethodSymbol).Parameters.First();

				bool firstIsString = parameters.Type.Name == typeof(string).Name;
			}

			var notifyMethod = classMembers.FirstOrDefault(m => m.Kind == SymbolKind.Method && m.Name == "NotifyOfChange"
							&& (m as IMethodSymbol).Parameters.Length == 1 && (m as IMethodSymbol).Parameters.First().Type.Name == typeof(string).Name);

			string methodName;
			if (notifyMethod != null)
			{
				methodName = "NotiFire_Fallback_Notify_Method";
			}
			else
			{
				methodName = "NotifyOfChange";
			}

			notifySource.AppendLine($"\t\tprotected void {methodName}(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");

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

				if (string.IsNullOrWhiteSpace(name))
				{
					name = $"{field.Name}Property";
				}

				string comments = field.GetDocumentationCommentXml();
				if (!string.IsNullOrWhiteSpace(comments))
				{
					notifySource.AppendLine(comments);
				}

				notifySource.AppendLine($@"
		public {field.Type} {name}
		{{
			get => {field.Name};
			set {{ {field.Name} = value; {methodName}(nameof({name})); }}
		}}");
				notifySource.AppendLine();
			}

			notifySource.AppendLine("\t}");
			notifySource.AppendLine("}");

			return notifySource.ToString();
		}
	}
}
