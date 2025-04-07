using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Kairou.SourceGenerator;

public static class CommandSymbolUtil
{
    public static IMethodSymbol? GetExecuteMethod(INamedTypeSymbol commandTypeSymbol, Compilation compilation)
    {
        var executeAttribute = Symbols.CommandExecuteAttribute(compilation);
        foreach (var m in commandTypeSymbol.GetMembers())
        {
            if (m is not IMethodSymbol methodSymbol) continue;
            foreach (AttributeData attr in m.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, executeAttribute))
                {
                    return methodSymbol;
                }
            }
        }
        return null;
    }
}
