using Bank.Messages;
using OpenSleigh.Core;
using OpenSleigh.Core.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bank.Saga.Withdrawal
{
    public record AccountValidated(Guid Id, Guid CorrelationId, WithdrawalMessage message) : ICommand { }
    public record WithdrawalApproved(Guid Id, Guid CorrelationId, WithdrawalMessage message) : ICommand { }
    public record AccountBalanceUpdated(Guid Id, Guid CorrelationId, WithdrawalMessage message) : ICommand { }
    public record WithdrawalProcessed(Guid Id, Guid CorrelationId, WithdrawalMessage message) : ICommand { }
    public record WithdrawalCompleted(Guid Id, Guid CorrelationId, WithdrawalMessage message) : ICommand { }

    public class WithdrawalSaga :
        Saga<WithdrawalInitiatedState>,
        IStartedBy<AccountValidated>,
        IHandleMessage<WithdrawalApproved>,
        IHandleMessage<AccountBalanceUpdated>,
        IHandleMessage<WithdrawalProcessed>,
        IHandleMessage<WithdrawalCompleted>
    {

        public async Task HandleAsync(IMessageContext<AccountValidated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"1. Account Validated: {context.Message.message.AccountNumber}");
            var message = new WithdrawalApproved(context.Message.Id, context.Message.CorrelationId, context.Message.message);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<WithdrawalApproved> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"2. Withdrawal Approved: {context.Message.message.AccountNumber}");
            var message = new AccountBalanceUpdated(context.Message.Id, context.Message.CorrelationId, context.Message.message);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<AccountBalanceUpdated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"3. Account Balance Updated: {context.Message.message.AccountNumber}");
            var message = new WithdrawalProcessed(context.Message.Id, context.Message.CorrelationId, context.Message.message);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<WithdrawalProcessed> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"4. Withdrawal Processed: {context.Message.message.AccountNumber}");
            var message = new WithdrawalCompleted(context.Message.Id, context.Message.CorrelationId, context.Message.message);
            this.Publish(message);            
        }

        public Task HandleAsync(IMessageContext<WithdrawalCompleted> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"5. Withdrawal Complete: {context.Message.message.AccountNumber}");
            this.State.MarkAsCompleted();
            return Task.CompletedTask;
        }        
    }
}
