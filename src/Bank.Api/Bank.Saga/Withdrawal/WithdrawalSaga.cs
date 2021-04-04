using Bank.Messages;
using OpenSleigh.Core;
using OpenSleigh.Core.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bank.Saga.Withdrawal
{
    public class WithdrawalSagaState : SagaState
    {
        public WithdrawalSagaState(Guid id)
            : base(id)
        {
        }

        public string AccountNumber { get; set; }
        public double Amount { get; set; }
    } 

    public record AccountValidated(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalApproved(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record AccountBalanceUpdated(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalProcessed(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalCompleted(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }

    public class WithdrawalSaga :
        Saga<WithdrawalSagaState>,
        IStartedBy<AccountValidated>,
        IHandleMessage<WithdrawalApproved>,
        IHandleMessage<AccountBalanceUpdated>,
        IHandleMessage<WithdrawalProcessed>,
        IHandleMessage<WithdrawalCompleted>
    {

        public async Task HandleAsync(IMessageContext<AccountValidated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"1. Account Validated: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new WithdrawalApproved(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<WithdrawalApproved> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"2. Withdrawal Approved: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new AccountBalanceUpdated(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<AccountBalanceUpdated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"3. Account Balance Updated: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new WithdrawalProcessed(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<WithdrawalProcessed> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"4. Withdrawal Processed: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new WithdrawalCompleted(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);            
        }

        public Task HandleAsync(IMessageContext<WithdrawalCompleted> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"5. Withdrawal Complete: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            this.State.MarkAsCompleted();
            return Task.CompletedTask;
        }        
    }
}
