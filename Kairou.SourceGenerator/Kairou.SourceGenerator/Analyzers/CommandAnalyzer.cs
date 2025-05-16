using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kairou.SourceGenerator;

[DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class CommandAnalyzer : DiagnosticAnalyzer
{
    static string Category = Const.ProjectName;

    public static readonly DiagnosticDescriptor CommandMustBePartialClass = new(
        id: "Kairou0001",
        title: "Command class must be declared as partial.",
        messageFormat: "'{0}' must be declared as partial.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandHasNoExecuteMethod = new(
        id: "Kairou0002",
        title: "This command has no [CommandExecute] method.",
        messageFormat: "'{0}' has no [CommandExecute] method.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ExecuteMethodOfAsyncCommandMustReturnUnitask = new(
        id: "Kairou0003",
        title: "[CommandExecute] method of AsyncCommand must return UniTask.",
        messageFormat: "[CommandExecute] method of AsyncCommand must return UniTask.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandMustNotBeInnerClass = new(
        id: "Kairou0004",
        title: "Command class must not be inner class.",
        messageFormat: "Command class must not be inner class.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ExecuteMethodParameterIsDefault = new(
        id: "Kairou0005",
        title: "The parameter receives a default value. To specify how the argument should be resolved, apply the corresponding attribute.",
        messageFormat: "The parameter '{0}' receives a default value. To specify how the argument should be resolved, apply the corresponding attribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get => ImmutableArray.Create(
            CommandMustBePartialClass,
            CommandHasNoExecuteMethod,
            ExecuteMethodOfAsyncCommandMustReturnUnitask,
            CommandMustNotBeInnerClass,
            ExecuteMethodParameterIsDefault);
    }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var compilation = context.Compilation;

        var symbol = (INamedTypeSymbol)context.Symbol;
        if (symbol.IsSubclassOf(Symbols.Command(compilation)) == false) return;
        if (symbol.IsAbstract) return;

        var fields = symbol.GetMembers().OfType<IFieldSymbol>();

        // Commandが内部クラスだとコード生成が面倒なので、とりあえず内部クラスは禁じる
        if (symbol.ContainingType != null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    CommandMustNotBeInnerClass,
                    symbol.GetLocation()
                )
            );
            return;
        }

        if (symbol.IsPartial() == false)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    CommandMustBePartialClass,
                    symbol.GetLocation(),
                    symbol.Name
                )
            );
        }

        IMethodSymbol? executeMethod = CommandSymbolUtil.GetExecuteMethod(symbol, compilation);

        if (executeMethod == null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    CommandHasNoExecuteMethod,
                    symbol.GetLocation(),
                    symbol.Name
                )
            );
            return;
        }

        bool isAsyncCommand = symbol.IsSubclassOf(Symbols.AsyncCommand(compilation));

        if (isAsyncCommand)
        {
            if (SymbolEqualityComparer.Default.Equals(executeMethod.ReturnType, Symbols.UniTask(compilation)) == false)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        ExecuteMethodOfAsyncCommandMustReturnUnitask,
                        executeMethod.GetLocation()
                    )
                );
                return;
            }
        }

        var parameters = executeMethod.Parameters;
        // Generatorの引数解決と対応
        foreach (IParameterSymbol parameter in parameters)
        {
            var attributes = parameter.GetAttributes();
            var injectAttribute = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, Symbols.InjectAttribute(compilation)));
            var fromAttribute = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, Symbols.FromAttribute(compilation)));

            if (injectAttribute != null) continue;
            if (fromAttribute != null)
            {
                var args = fromAttribute.ConstructorArguments;
                string fieldName = (string)args[0].Value!;
                var field = fields.First(x => x.Name == fieldName);
                if (field.Type.IsSubclassOf(Symbols.VariableKey(compilation)))
                {
                    continue;
                }
                else if (field.Type.IsSubclassOf(Symbols.VariableValueGetterKey(compilation)))
                {
                    continue;
                }
                else if (field.Type.IsSubclassOf(Symbols.VariableValueAccessorKey(compilation)))
                {
                    continue;
                }
                else if (field.Type.IsSubclassOf(Symbols.FlexibleParameter(compilation)))
                {
                    continue;
                }
            }
            if (attributes.Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, Symbols.InjectAttribute(compilation))))
            {
                continue;
            }
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, Symbols.IProcessInterface(compilation)))
            {
                continue;
            }
            if (isAsyncCommand && SymbolEqualityComparer.Default.Equals(parameter.Type, Symbols.CancellationToken(compilation)))
            {
                continue;
            }
            // パラメータが解決できない場合、defaultが渡される
            context.ReportDiagnostic(
                Diagnostic.Create(
                    ExecuteMethodParameterIsDefault,
                    parameter.GetLocation(),
                    parameter.Name
                )
            );
        }
    }
}
