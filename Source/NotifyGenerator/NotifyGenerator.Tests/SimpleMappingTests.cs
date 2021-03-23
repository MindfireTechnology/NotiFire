using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Reflection;
using Shouldly;
using Xunit;
using System.Linq;

namespace NotifyGenerator.Tests
{
	public class SimpleMappingTests
	{
		[Fact]
		public void Test1()
		{
			// Arrange
			var file = File.ReadAllText(@"./classes/AllClasses.txt");
			var compilation = CreateCompilation(file);
			var generator = new MapperGenerator();
			var driver = CSharpGeneratorDriver.Create(generator);

			// Act
			driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

			// Assert
			diagnostics.IsDefaultOrEmpty.ShouldBeTrue();
			var outputDiag = outputCompilation.GetDiagnostics();
			outputDiag.IsEmpty.ShouldBeTrue();

			var iface = outputCompilation.SyntaxTrees.SingleOrDefault(st => st.FilePath.EndsWith("\\IMapper.cs"));
			var cls = outputCompilation.SyntaxTrees.SingleOrDefault(st => st.FilePath.EndsWith("\\Mapper.cs"));

			iface.ShouldNotBeNull();
			cls.ShouldNotBeNull();

			string ifaceStr = iface.ToString();
			string clsStr = cls.ToString();

			// TODO: Assert something interesting about the generated code
		}

		private static Compilation CreateCompilation(string source)
			=> CSharpCompilation.Create("compilation",
				new[] { CSharpSyntaxTree.ParseText(source) },
				new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}


}
