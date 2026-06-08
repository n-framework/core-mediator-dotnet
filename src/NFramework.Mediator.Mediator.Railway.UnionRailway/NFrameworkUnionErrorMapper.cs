using Microsoft.AspNetCore.Http;
using UnionRailway;
using UnionRailway.AspNetCore;

namespace NFramework.Mediator.Mediator.Railway.UnionRailway;

/// <summary>
/// Default <see cref="IUnionErrorMapper"/> for NFramework that defers to UnionRailway's built-in
/// RFC 7807 mapping. It exists so applications can register the railway integration in one call and
/// later replace it with a customized mapper without changing call sites.
/// </summary>
public sealed class NFrameworkUnionErrorMapper : IUnionErrorMapper
{
    /// <summary>
    /// Returns <c>null</c> to fall back to the default UnionRailway problem-details mapping for every error.
    /// </summary>
    /// <param name="error">The error to translate.</param>
    /// <returns>Always <c>null</c>; the default mapping is used.</returns>
    public IResult? TryMap(UnionError error) => null;
}
