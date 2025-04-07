using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kairou.SourceGenerator;

public static class NamedTypeSymbolExtensions
{
    public static bool IsSubclassOf(this INamedTypeSymbol self,  INamedTypeSymbol other)
    {
        var baseType = self.BaseType;
        while (baseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, other))
                return true;

            baseType = baseType.BaseType;
        }
        return false;
    }

    public static bool IsPartial(this INamedTypeSymbol self)
    {
        foreach (var syntaxRef in self.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not TypeDeclarationSyntax typeDecl) continue;

            for (int i = 0; i < typeDecl.Modifiers.Count; i++)
            {
                if (typeDecl.Modifiers[i].IsKind(SyntaxKind.PartialKeyword)) return true;
            }
            
        }
        return false;
    }
}
