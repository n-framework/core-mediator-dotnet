using System.Diagnostics.CodeAnalysis;

namespace NFramework.Mediator.Abstractions.Transactions;

[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for transactional requests."
)]
public interface ITransactionalRequest { }
