namespace NFramework.Mediator.Mediator.Transactions;

/// <summary>
/// Configuration options for NFramework transaction behavior.
/// </summary>
public class MediatorTransactionOptions
{
    /// <summary>
    /// The timeout for transaction scopes. Default is 30 seconds.
    /// </summary>
    public TimeSpan TransactionScopeTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
