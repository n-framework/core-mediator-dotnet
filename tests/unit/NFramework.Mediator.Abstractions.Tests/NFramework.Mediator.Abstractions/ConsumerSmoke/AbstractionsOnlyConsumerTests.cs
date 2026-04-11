namespace NFramework.Mediator.Abstractions.Tests.ConsumerSmoke;

public sealed class AbstractionsOnlyConsumerTests
{
    [Fact]
    public void ConsumerAssert_CompilesAgainstAbstractionsOnlyContracts()
    {
        var consumerType = typeof(AbstractionsOnlyConsumer);

        _ = consumerType.ShouldNotBeNull();
    }
}
