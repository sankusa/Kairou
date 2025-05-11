using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Kairou.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CommandIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandClassDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => ((ClassDeclarationSyntax)context.Node, context.SemanticModel))
            .Select(static (tuple, ct) =>
            {
                var (classNode, semanticModel) = tuple;

                var compilation = semanticModel.Compilation;

                var classSymbol = semanticModel.GetDeclaredSymbol(classNode, ct) as INamedTypeSymbol;
                if (classSymbol == null) return null;

                if (classSymbol.IsSubclassOf(Symbols.Command(compilation)) == false) return null;
                if (classSymbol.IsAbstract) return null;
                return classSymbol;
            })
            .Where(static x => x != null)
            .Select(static (x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(commandClassDeclarations),
            static (context, tuple) =>
            {
                Compilation compilation = tuple.Left;
                ImmutableArray<INamedTypeSymbol> commandTypeSymbols = tuple.Right;
                foreach (var commandTypeSymbol in commandTypeSymbols)
                {
                    Emit(context, compilation, commandTypeSymbol);
                }
            });
    }

    static void Emit(SourceProductionContext context, Compilation compilation, INamedTypeSymbol commandTypeSymbol)
    {
        bool isAsyncCommand = commandTypeSymbol.IsSubclassOf(Symbols.AsyncCommand(compilation));

        INamedTypeSymbol commandExecuteAttribute = Symbols.CommandExecuteAttribute(compilation);

        if (commandTypeSymbol.IsPartial() == false) return;

        IMethodSymbol? executeMethod = CommandSymbolUtil.GetExecuteMethod(commandTypeSymbol, compilation);

        if (executeMethod == null) return;

        string typeFullName = commandTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string typeFullNameSafe = typeFullName
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_");

        var ns = commandTypeSymbol.ContainingNamespace;

        var fields = commandTypeSymbol
            .GetMembers()
            .OfType<IFieldSymbol>();
        var parameters = executeMethod.Parameters;

        var builder = new CodeBuilder();
        builder.AppendIndentedLine("using Kairou;");
        builder.AppendIndentedLine("using System.Collections.Generic;");
        if (isAsyncCommand)
        {
            builder.AppendIndentedLine("using System.Threading;");
            builder.AppendIndentedLine("using Cysharp.Threading.Tasks;");
        }
        builder.AppendLine();

        if (ns.IsGlobalNamespace == false)
        {
            builder.AppendIndentedLine($"namespace {ns}");
            builder.BeginBlock();
        }

        builder.AppendIndentedLine($"partial class {commandTypeSymbol.Name}");
        using (new BlockScope(builder))
        {
            if (isAsyncCommand)
            {
                builder.AppendIndentedLine($"public override async UniTask InvokeExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)");
            }
            else
            {
                builder.AppendIndentedLine($"public override void InvokeExecute(IProcessInterface process)");
            }
            using (new BlockScope(builder))
            {
                BuildInvokeExecuteBody(builder, isAsyncCommand, compilation, fields, executeMethod, parameters);
            }

            builder.AppendLine();
            BuildValidate_Generated(builder, isAsyncCommand, compilation, fields);
        }

        if (ns.IsGlobalNamespace == false)
        {
            builder.EndBlock();
        }

        context.AddSource($"{typeFullNameSafe}.Generated.g.cs", builder.ToString());
    }

    static void BuildInvokeExecuteBody(CodeBuilder builder, bool isAsyncCommand, Compilation compilation, IEnumerable<IFieldSymbol> fields, IMethodSymbol executeMethod, ImmutableArray<IParameterSymbol> parameters)
    {
        

        var paramListBuilder = new StringBuilder();

        foreach (IParameterSymbol parameter in parameters)
        {
            var attributes = parameter.GetAttributes();
            var injectAttribute = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, Symbols.InjectAttribute(compilation)));
            var fromAttribute = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, Symbols.FromAttribute(compilation)));

            if (injectAttribute != null)
            {
                builder.AppendIndentedLine($"var {parameter.Name} = process.Resolve<{parameter.Type.ToDisplayString()}>();");
                if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                paramListBuilder.Append(parameter.Name);
                continue;
            }
            if(fromAttribute != null)
            {
                var args = fromAttribute.ConstructorArguments;
                string fieldName = (string)args[0].Value!;
                var field = fields.First(x => x.Name == fieldName);
                if (field != null)
                {
                    if (field.Type.IsSubclassOf(Symbols.VariableKey(compilation)))
                    {
                        builder.AppendIndentedLine($"var {parameter.Name} = {fieldName}.Find(process).Value;");
                        if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                        paramListBuilder.Append(parameter.Name);
                        continue;
                    }
                    else if (field.Type.IsSubclassOf(Symbols.VariableValueGetterKey(compilation)))
                    {
                        builder.AppendIndentedLine($"var {parameter.Name} = {fieldName}.Find(process).GetValue();");
                        if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                        paramListBuilder.Append(parameter.Name);
                        continue;
                    }
                    else if (field.Type.IsSubclassOf(Symbols.VariableValueAccessorKey(compilation)))
                    {
                        builder.AppendIndentedLine($"var {parameter.Name} = {fieldName}.Find(process).GetValue();");
                        if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                        paramListBuilder.Append(parameter.Name);
                        continue;
                    }
                    else if (field.Type.IsSubclassOf(Symbols.FlexibleParameter(compilation)))
                    {
                        builder.AppendIndentedLine($"var {parameter.Name} = {fieldName}.ResolveValue(process);");
                        if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                        paramListBuilder.Append(parameter.Name);
                        continue;
                    }
                }
                
            }
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, Symbols.IProcessInterface(compilation)))
            {
                if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                paramListBuilder.Append("process");
                continue;
            }
            if(isAsyncCommand && SymbolEqualityComparer.Default.Equals(parameter.Type, Symbols.CancellationToken(compilation)))
            {
                if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
                paramListBuilder.Append("cancellationToken");
                continue;
            }
            
            if (paramListBuilder.Length > 0) paramListBuilder.Append(", ");
            paramListBuilder.Append($"default({parameter.Type.ToDisplayString()})");
        }

        builder.AppendIndent();
        if (isAsyncCommand)
        {
            builder.Append("await ");
        }
        builder.Append($"{executeMethod.Name}(");
        builder.Append(paramListBuilder);
        builder.AppendLine(");");
    }

    static void BuildValidate_Generated(CodeBuilder builder, bool isAsyncCommand, Compilation compilation, IEnumerable<IFieldSymbol> fields)
    {
        builder.AppendIndentedLine("protected override IEnumerable<string> Validate_Generated()");
        using (new BlockScope(builder))
        {
            foreach (var field in fields)
            {
                if (field.Type.IsSubclassOf(Symbols.VariableKey(compilation))
                    || field.Type.IsSubclassOf(Symbols.VariableValueGetterKey(compilation))
                    || field.Type.IsSubclassOf(Symbols.VariableValueSetterKey(compilation))
                    || field.Type.IsSubclassOf(Symbols.VariableValueAccessorKey(compilation))
                    || field.Type.IsSubclassOf(Symbols.FlexibleParameter(compilation)))
                {
                    builder.AppendIndentedLine($"foreach (string errorMessage in {field.Name}.Validate(this, nameof({field.Name})))");
                    using (new BlockScope(builder))
                    {
                        builder.AppendIndentedLine($"yield return errorMessage;");
                    }
                    builder.AppendLine();
                }
            }
            builder.AppendIndentedLine($"yield break;");
        }
    }
}