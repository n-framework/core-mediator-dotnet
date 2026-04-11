namespace NFramework.Mediator.Configuration;

/// <summary>
/// Execution order values for pipeline behaviors.
/// </summary>
public enum BehaviorOrder
{
    /// <summary>
    /// Logging behavior executes first (priority 100).
    /// </summary>
    Logging = 100,

    /// <summary>
    /// Validation behavior executes second (priority 200).
    /// </summary>
    Validation = 200,

    /// <summary>
    /// Transaction behavior executes last (priority 300).
    /// </summary>
    Transaction = 300,
}
