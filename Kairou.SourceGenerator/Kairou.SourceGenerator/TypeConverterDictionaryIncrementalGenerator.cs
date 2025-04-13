using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kairou.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class TypeConverterDictionaryIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeConverterDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => ((ClassDeclarationSyntax)context.Node, context.SemanticModel))
            .Select(static (tuple, ct) =>
            {
                var (classNode, semanticModel) = tuple;

                var compilation = semanticModel.Compilation;

                var classSymbol = semanticModel.GetDeclaredSymbol(classNode, ct) as INamedTypeSymbol;
                if (classSymbol == null) return null;

                if (classSymbol.IsSubclassOf(Symbols.TypeConverter(compilation)) == false) return null;
                if (classSymbol.IsAbstract || classSymbol.IsGenericType) return null;
                return classSymbol;
            })
            .Where(static x => x != null)
            .Select(static (x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(typeConverterDeclarations),
            static (context, tuple) =>
            {
                Compilation compilation = tuple.Left;
                ImmutableArray<INamedTypeSymbol> typeConverterTypeSymbols = tuple.Right;
                Emit(context, compilation, typeConverterTypeSymbols);
            });

        static void Emit(SourceProductionContext context, Compilation compilation, ImmutableArray<INamedTypeSymbol> typeConverterTypeSymbols)
        {
            if (typeConverterTypeSymbols.Length == 0) return;

            string typeFullName = Symbols.TypeConverterDictionary(compilation).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string typeFullNameSafe = typeFullName
                .Replace("global::", "")
                .Replace("<", "_")
                .Replace(">", "_");

            var builder = new CodeBuilder();
            builder.AppendIndentedLine($"using UnityEngine;");
            builder.AppendLine($"#if UNITY_EDITOR");
            builder.AppendIndentedLine($"using UnityEditor;");
            builder.AppendLine($"#endif");
            builder.AppendLine();
            builder.AppendIndentedLine($"namespace Kairou.Generated");
            using (new BlockScope(builder))
            {
                builder.AppendIndentedLine($"static class TypeConverterRegistrar");
                using (new BlockScope(builder))
                {
                    builder.AppendIndentedLine($"static bool _isRegistered;");
                    builder.AppendLine();
                    builder.AppendLine($"#if UNITY_EDITOR");
                    builder.AppendIndentedLine($"[InitializeOnLoadMethod]");
                    builder.AppendLine($"#endif");
                    builder.AppendIndentedLine($"[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]");
                    builder.AppendIndentedLine($"static void Register()");
                    using (new BlockScope(builder))
                    {
                        builder.AppendIndentedLine($"if (_isRegistered) return;");
                        builder.AppendIndentedLine($"_isRegistered = true;");
                        foreach (var typeConverterTypeSymbol in typeConverterTypeSymbols)
                        {
                            builder.AppendIndentedLine($"new {typeConverterTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}().Register();");
                        }
                    }
                }
            }
            context.AddSource($"{compilation.AssemblyName}..{typeFullNameSafe}.Generated.g.cs", builder.ToString());
        }
    }
}