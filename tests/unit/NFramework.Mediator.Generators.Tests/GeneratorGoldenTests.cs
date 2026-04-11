using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NFramework.Mediator.Generators.Generation;
using Xunit;

namespace NFramework.Mediator.Generators.Tests;

public sealed class GeneratorGoldenTests
{
    [Fact]
    public void CommandHandlerRegistration_MatchesGolden()
    {
        string source = """
using System.Threading;
using System.Threading.Tasks;
using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Requests;

public sealed class CreateOrderCommand : ICommand<int>;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, int>
{
    public ValueTask<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(1);
}
""";

        string generated = RunGenerator(source, "MediatorExtensions.g.cs");
        Assert.Equal(ReadGolden("command-registration.txt"), Normalize(generated));
    }

    [Fact]
    public void QueryHandlerRegistration_MatchesGolden()
    {
        string source = """
using System.Threading;
using System.Threading.Tasks;
using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Requests;

public sealed class GetOrderQuery : IQuery<int>;

public sealed class GetOrderHandler : IQueryHandler<GetOrderQuery, int>
{
    public ValueTask<int> Handle(GetOrderQuery query, CancellationToken cancellationToken) => ValueTask.FromResult(1);
}
""";

        string generated = RunGenerator(source, "MediatorExtensions.g.cs");
        Assert.Equal(ReadGolden("query-registration.txt"), Normalize(generated));
    }

    [Fact]
    public void EventHandlerRegistration_MatchesGolden()
    {
        string source = """
using System.Threading;
using System.Threading.Tasks;
using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Requests;

public sealed class OrderCreated : IEvent;

public sealed class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public ValueTask Handle(OrderCreated @event, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
""";

        string generated = RunGenerator(source, "MediatorExtensions.g.cs");
        Assert.Equal(ReadGolden("event-registration.txt"), Normalize(generated));
    }

    [Fact]
    public void RouteMappings_MatchesGolden()
    {
        string source = """
using System;
using System.Threading;
using System.Threading.Tasks;
using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Requests;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ApiExposedAttribute : Attribute;

public sealed class CreateOrderCommand : ICommand<int>;
public sealed class GetOrderQuery : IQuery<int>;

[ApiExposed]
public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, int>
{
    public ValueTask<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(1);
}

[ApiExposed]
public sealed class GetOrderHandler : IQueryHandler<GetOrderQuery, int>
{
    public ValueTask<int> Handle(GetOrderQuery query, CancellationToken cancellationToken) => ValueTask.FromResult(1);
}
""";

        string generated = RunGenerator(source, "RouteMappings.g.cs");
        Assert.Equal(ReadGolden("route-mappings.txt"), Normalize(generated));
    }

    [Fact]
    public void EmptyHandlers_CompilesAndGeneratesEmptyRegistrationBlock()
    {
        string source = "public sealed class Nothing;";
        string generated = RunGenerator(source, "MediatorExtensions.g.cs");

        Assert.Contains("AddMediatorGeneratedHandlers", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("AddTransient<", generated, StringComparison.Ordinal);
    }

    private static string RunGenerator(string source, string generatedHintName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ValueTask).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(
                typeof(NFramework.Mediator.Abstractions.Contracts.Requests.ICommand<>).Assembly.Location
            ),
            MetadataReference.CreateFromFile(typeof(MediatorGenerator).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorTests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        ISourceGenerator generator = new MediatorGenerator().AsSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator],
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview)
        );
        driver = driver.RunGenerators(compilation);
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        string? text = runResult
            .GeneratedTrees.FirstOrDefault(tree => tree.FilePath.EndsWith(generatedHintName, StringComparison.Ordinal))
            ?.GetText()
            .ToString();

        return text ?? throw new InvalidOperationException($"Generated file '{generatedHintName}' was not found.");
    }

    private static string ReadGolden(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Golden", fileName);
        return Normalize(File.ReadAllText(path));
    }

    private static string Normalize(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
}
