namespace NFramework.Mediator.Abstractions.Tests.ConsumerSmoke;

public sealed class AbstractionsOnlyConsumerTests
{
    [Fact]
    public void ConsumerAsset_CompilesAgainstAbstractionsOnlyContracts()
    {
        var consumerType = typeof(AbstractionsOnlyConsumer);

        Assert.NotNull(consumerType);
    }
}
