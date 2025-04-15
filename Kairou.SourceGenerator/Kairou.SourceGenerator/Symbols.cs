using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Kairou.SourceGenerator;

public static class Symbols
{
    public static INamedTypeSymbol UniTask(Compilation compilation) => compilation.GetTypeByMetadataName("Cysharp.Threading.Tasks.UniTask")!;
    public static INamedTypeSymbol CancellationToken(Compilation compilation) => compilation.GetTypeByMetadataName("System.Threading.CancellationToken")!;

    public static INamedTypeSymbol PageProcess(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.PageProcess")!;
    public static INamedTypeSymbol Command(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.Command")!;
    public static INamedTypeSymbol AsyncCommand(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.AsyncCommand")!;
    public static INamedTypeSymbol CommandExecuteAttribute(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.CommandExecuteAttribute")!;
    public static INamedTypeSymbol InjectAttribute(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.InjectAttribute")!;

    public static INamedTypeSymbol VariableType(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.VariableType")!;
    public static INamedTypeSymbol VariableTypeDictionary(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.VariableTypeDictionary")!;

    public static INamedTypeSymbol TypeConverter(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.TypeConverter")!;
    public static INamedTypeSymbol TypeConverterDictionary(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.TypeConverterDictionary")!;

    public static INamedTypeSymbol VariableKey(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.VariableKey")!;
    public static INamedTypeSymbol VariableValueGetterKey(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.VariableValueGetterKey")!;
    public static INamedTypeSymbol VariableValueSetterKey(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.VariableValueSetterKey")!;
    public static INamedTypeSymbol VariableValueAccessorKey(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.VariableValueAccessorKey")!;
    public static INamedTypeSymbol FlexibleParameter(Compilation compilation) => compilation.GetTypeByMetadataName("Kairou.FlexibleParameter")!;
}
