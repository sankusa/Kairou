using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kairou.SourceGenerator.Tests;

internal class Config
{
    public static readonly string DummyClassesDllPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\DummyClasses\bin\Debug\netstandard2.0\DummyClasses.dll"));
}
