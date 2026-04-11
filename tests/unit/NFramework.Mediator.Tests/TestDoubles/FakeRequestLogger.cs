using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeRequestLogger<TRequest> : IRequestLogger<TRequest>
{
    public int StartCallCount { get; private set; }

    public int CompleteCallCount { get; private set; }

    public int FailedCallCount { get; private set; }

    public int ShortCircuitCallCount { get; private set; }

    public void RequestStarted(TRequest request)
    {
        StartCallCount++;
    }

    public void RequestCompleted(TRequest request, TimeSpan elapsed)
    {
        CompleteCallCount++;
    }

    public void RequestFailed(TRequest request, Exception exception, TimeSpan elapsed)
    {
        FailedCallCount++;
    }

    public void RequestShortCircuited(TRequest request, TimeSpan elapsed)
    {
        ShortCircuitCallCount++;
    }
}
