using Microsoft.CodeAnalysis.CSharp.Testing;
using Kairou.SourceGenerator;
using Microsoft.CodeAnalysis.Testing;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Kairou.SourceGenerator.CommandAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using Microsoft.CodeAnalysis;

namespace Kairou.SourceGenerator.Tests
{
    public class Tests
    {
        [Test]
        public async Task CommandMustBePartialClass()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public class TestCommand : Command
{
    [CommandExecute]
    void XXXX()
    {
        int x = 3;
    }
}
"""},
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic(CommandAnalyzer.CommandMustBePartialClass)
                            .WithSpan(3, 14, 3, 25)
                            .WithArguments("TestCommand")
                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }

        [Test]
        public async Task CommandHasNoExecuteMethod()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public partial class TestCommand : Command
{
    void XXXX()
    {
        int x = 3;
    }
}
"""},
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic(CommandAnalyzer.CommandHasNoExecuteMethod)
                            .WithSpan(3, 22, 3, 33)
                            .WithArguments("TestCommand")
                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }

        [Test]
        public async Task ExecuteMethodOfAsyncCommandMustReturnUnitask()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public partial class TestCommand : AsyncCommand
{
    [CommandExecute]
    void XXXX()
    {
        int x = 3;
    }
}
"""},
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic(CommandAnalyzer.ExecuteMethodOfAsyncCommandMustReturnUnitask)
                            .WithSpan(6, 10, 6, 14)
                            .WithArguments("TestCommand")
                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }

        [Test]
        public async Task CommandMustNotBeInnerClass()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public class Boss
{
    public partial class TestCommand : AsyncCommand
    {
        [CommandExecute]
        void XXXX()
        {
            int x = 3;
        }
    }
}
"""},
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic(CommandAnalyzer.CommandMustNotBeInnerClass)
                            .WithSpan(5, 26, 5, 37)
                            .WithArguments("TestCommand")
                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }

        [Test]
        public async Task ExecuteMethodParameterIsDefault()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public partial class TestCommand : Command
{
    [CommandExecute]
    void XXXX(Command command)
    {
        int x = 3;
    }
}
"""},
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic(CommandAnalyzer.ExecuteMethodParameterIsDefault)
                            .WithSpan(6, 23, 6, 30)
                            .WithArguments("command")
                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }

        [Test]
        public async Task ExecuteMethodParameterIsDefault_Multi()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public partial class TestCommand : Command
{
    [CommandExecute]
    void XXXX(Command command, [Inject] AsyncCommand asyncCommand, InjectAttribute attribute)
    {
        int x = 3;
    }
}
"""},
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic(CommandAnalyzer.ExecuteMethodParameterIsDefault)
                            .WithSpan(6, 23, 6, 30)
                            .WithArguments("command"),
                        Verify.Diagnostic(CommandAnalyzer.ExecuteMethodParameterIsDefault)
                            .WithSpan(6, 84, 6, 93)
                            .WithArguments("attribute")
                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }

        [Test]
        public async Task Normal()
        {
            var test = new CSharpAnalyzerTest<CommandAnalyzer, DefaultVerifier>()
            {
                TestState =
                {
                    Sources =
                    {"""
using Kairou;

public partial class TestCommand : Command
{
    [CommandExecute]
    void XXXX([Inject] Command command)
    {
        int x = 3;
    }
}
"""},
                    ExpectedDiagnostics =
                    {

                    },
                }

            };
            test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(Config.DummyClassesDllPath));

            await test.RunAsync();
        }
    }
}