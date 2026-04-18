using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FluentRoslyn.Tests
{
    internal static class TestHelper
    {
        private static readonly IReadOnlyList<MetadataReference> DefaultReferences = CreateDefaultReferences();

        private static IReadOnlyList<MetadataReference> CreateDefaultReferences()
        {
            var trustedPaths = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            if (trustedPaths != null)
            {
                return trustedPaths
                    .Split(Path.PathSeparator)
                    .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                    .ToList();
            }

            return new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DescriptionAttribute).Assembly.Location),
            };
        }

        public static CSharpCompilation CreateCompilation(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: DefaultReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        public static INamedTypeSymbol GetTypeSymbol(CSharpCompilation compilation, string typeName)
        {
            var symbol = compilation.GetTypeByMetadataName(typeName);
            if (symbol is null)
                throw new System.Exception($"Type '{typeName}' not found in compilation.");
            return symbol;
        }
    }
}
