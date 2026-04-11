namespace NFramework.Mediator.Behaviors;

public interface IRequestLogger<TRequest>
{
    void RequestStarted(TRequest request);

    void RequestCompleted(TRequest request, TimeSpan elapsed);

    void RequestFailed(TRequest request, Exception exception, TimeSpan elapsed);

    void RequestShortCircuited(TRequest request, TimeSpan elapsed);
}

public interface IShortCircuitResult
{
    bool IsShortCircuited { get; }
}
