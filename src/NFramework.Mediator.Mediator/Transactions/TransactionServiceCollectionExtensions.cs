using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace NFramework.Mediator.Mediator.Transactions;

public static class TransactionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the NFramework Transaction behavior.
        /// </summary>
        public IServiceCollection AddNFrameworkTransactions()
        {
            _ = services.AddSingleton<MediatorTransactionOptions>();
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        }
    }
}
