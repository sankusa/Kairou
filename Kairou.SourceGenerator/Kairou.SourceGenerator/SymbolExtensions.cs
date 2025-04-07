using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Kairou.SourceGenerator;

public static class SymbolExtensions
{
    public static Location GetLocation(this ISymbol self)
    {
        return self.Locations.FirstOrDefault() ?? Location.None;
    }
}
