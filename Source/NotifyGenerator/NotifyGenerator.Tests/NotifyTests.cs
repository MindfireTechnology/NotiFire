using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Reflection;
using Shouldly;
using Xunit;
using System.Linq;
using NotiFire;

namespace NotifyGenerator.Tests
{
	public class NotifyTests
	{
		[Fact]
		public void Test1()
		{
			// Arrange
			var file = File.ReadAllText(@"./classes/AllClasses.txt");
			var compilation = CreateCompilation(file);
			var generator = new NotifyImplementationGenerator();
			var driver = CSharpGeneratorDriver.Create(generator);

			// Act
			driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

			// Assert
			//diagnostics.IsDefaultOrEmpty.ShouldBeTrue();
			var outputDiag = outputCompilation.GetDiagnostics();
			//outputDiag.IsEmpty.ShouldBeTrue();

			var allClasses = outputCompilation.SyntaxTrees.Where(st => st.FilePath.EndsWith("Notify.g.cs"));

			allClasses.ShouldNotBeNull("No classes were generated");

			allClasses.Count().ShouldBeGreaterThan(0, "No classes were generated");

			string first = allClasses.First().ToString();

			// TODO: Assert something interesting about the generated code
		}

		private static Compilation CreateCompilation(string source)
			=> CSharpCompilation.Create("compilation",
				new[] { CSharpSyntaxTree.ParseText(source) },
				new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}


}
