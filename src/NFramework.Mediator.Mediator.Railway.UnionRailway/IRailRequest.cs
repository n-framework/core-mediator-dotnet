using Mediator;
using UnionRailway;

namespace NFramework.Mediator.Mediator.Railway.UnionRailway;

/// <summary>
/// Marks a request whose response is a <see cref="Rail{TValue}"/>, enabling the railway pipeline
/// behaviors to short-circuit with a typed failure instead of throwing for expected problems.
/// </summary>
/// <typeparam name="TValue">The success value carried by the resulting rail.</typeparam>
public interface IRailRequest<TValue> : IRequest<Rail<TValue>>;
