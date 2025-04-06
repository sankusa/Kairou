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
}
