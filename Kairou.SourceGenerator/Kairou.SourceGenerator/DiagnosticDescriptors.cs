using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Kairou.SourceGenerator
{
    internal class DiagnosticDescriptors
    {
        static string Category = Const.ProjectName;

        public static readonly DiagnosticDescriptor UnexpectedError = new(
            id: "Kairou0000",
            title: "Unexpected error encountered in command class.",
            messageFormat: "An unexpected error occurred while processing '{0}'.\n{1}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
