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

        public bool isSuccessful { get; set; }
    } 

    public record WithdrawalInitiated(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalApproved(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record AccountBalanceUpdated(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalProcessed(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalCompleted(Guid Id, Guid CorrelationId, string AccountNumber, double Amount, bool isSuccessfull = true) : IMessage { }

    public class WithdrawalSaga :
        Saga<WithdrawalSagaState>,
        IStartedBy<WithdrawalInitiated>,
        IHandleMessage<WithdrawalApproved>,
        IHandleMessage<AccountBalanceUpdated>,        
        IHandleMessage<WithdrawalProcessed>,
        ICompensateMessage<WithdrawalProcessed>,
        IHandleMessage<WithdrawalCompleted>
    {        
        
        public async Task HandleAsync(IMessageContext<WithdrawalInitiated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"1. Withdrawal Initiated (Validate Account Number and check balances): {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new WithdrawalApproved(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<WithdrawalApproved> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"2. Withdrawal Approved (Update Account Balance) : {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new AccountBalanceUpdated(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);
        }

        public async Task HandleAsync(IMessageContext<AccountBalanceUpdated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"3. Account Balance Updated (Amount for withdrawal has been deducted from Account): {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");
            var message = new WithdrawalProcessed(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);
        }

     

        public async Task HandleAsync(IMessageContext<WithdrawalProcessed> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"4. Withdrawal Processed (Cash has been exchanged and sent to ATM): {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}");

            if (context.Message.Amount > 10000)
                throw new ApplicationException("You drawing to much money");

            var message = new WithdrawalCompleted(Guid.NewGuid(), context.Message.CorrelationId, context.Message.AccountNumber, context.Message.Amount);
            this.Publish(message);            
        }

        public async Task CompensateAsync(ICompensationContext<WithdrawalProcessed> context, CancellationToken cancellationToken = default)
        {
            bool isSuccessful = false;
            Console.WriteLine($"Withdrawal Processing Failed need to refund account number: {context.MessageContext.Message.AccountNumber} with amount: {context.MessageContext.Message.Amount}");
            var message = new WithdrawalCompleted(Guid.NewGuid(), context.MessageContext.Message.CorrelationId, context.MessageContext.Message.AccountNumber, context.MessageContext.Message.Amount, isSuccessful);
            this.Publish(message);
        }


        public Task HandleAsync(IMessageContext<WithdrawalCompleted> context, CancellationToken cancellationToken = default)
        {
            if (context.Message.isSuccessfull)
            {
                Console.WriteLine($"Withdrawal Completed Successfully: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber}  & Amount: {context.Message.Amount} ");                
            }
            else
            {
                Console.WriteLine($"Withdrawal Completed Failure: {context.Message.CorrelationId} & Account Number: {context.Message.AccountNumber} & Amount: {context.Message.Amount} ");
            }
            
            this.State.MarkAsCompleted();
            return Task.CompletedTask;
        }        
    }
}
