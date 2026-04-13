namespace NFramework.Mediator.Abstractions.Behaviors;

/// <summary>
/// Marker interface for the request logging behavior.
/// Only requests implementing this interface will be logged.
/// </summary>
public interface ILoggableRequest
{
    /// <summary>
    /// Parameter names to be excluded from logging (e.g. password, credit card).
    /// </summary>
    string[]? LogExcludeParameters { get; }
}
