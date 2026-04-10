using System.Reflection;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

internal static class ContractShapeInspector
{
    public static bool HasHandleMethod(Type contractType)
    {
        var methods = contractType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        return methods != null && methods.Any(method => method.Name == "Handle");
    }

    public static bool HasCancellationTokenParameter(MethodInfo method)
    {
        return method.GetParameters().Any(parameter => parameter.ParameterType == typeof(CancellationToken));
    }
}
