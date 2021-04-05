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

        public bool isWithdrawalValidated { get; set; }
        public bool isAccountBalanceUpdated { get; set; }
        public bool isWithdrawalApproved { get; set; }
        public bool isWithdrawalComplete { get; set; }
        public bool isSuccessful { get; set; }
    } 

    public record WithdrawalInitiated(Guid Id, Guid CorrelationId, string AccountNumber, double Amount) : IMessage { }
    public record WithdrawalValidated(Guid Id, Guid CorrelationId) : IMessage { }
    public record AccountBalanceUpdated(Guid Id, Guid CorrelationId) : IMessage { }
    public record WithdrawalApproved(Guid Id, Guid CorrelationId) : IMessage { }
    public record WithdrawalCompleted(Guid Id, Guid CorrelationId) : IMessage { }

    public class WithdrawalSaga :
        Saga<WithdrawalSagaState>,
        IStartedBy<WithdrawalInitiated>,
        IHandleMessage<WithdrawalValidated>,
        IHandleMessage<AccountBalanceUpdated>,        
        IHandleMessage<WithdrawalApproved>,
        ICompensateMessage<WithdrawalApproved>,
        IHandleMessage<WithdrawalCompleted>
    {        
        
        public async Task HandleAsync(IMessageContext<WithdrawalInitiated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"1. Withdrawal Initiated (Validate Account Number and check balances): {context.Message.CorrelationId} & Account Number: {State.AccountNumber}");

            State.AccountNumber = context.Message.AccountNumber;
            State.Amount = context.Message.Amount;
            var message1 = new WithdrawalValidated(Guid.NewGuid(), context.Message.CorrelationId);
            Publish(message1);

            var message2 = new AccountBalanceUpdated(Guid.NewGuid(), context.Message.CorrelationId);
            Publish(message2);
        }

        public async Task HandleAsync(IMessageContext<WithdrawalValidated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"2. Withdrawal Validated: {context.Message.CorrelationId} & Account Number: {State.AccountNumber}");
            
            this.State.isWithdrawalValidated = true;
            if (CanProcessWithdrawal(cancellationToken))
            {
                var message = new WithdrawalApproved(Guid.NewGuid(), context.Message.CorrelationId);
                Publish(message);
            }            
        }

        public async Task HandleAsync(IMessageContext<AccountBalanceUpdated> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"3. Account Balance Updated (Amount for withdrawal has been deducted from Account): {context.Message.CorrelationId} & Account Number: {State.AccountNumber}");
            this.State.isAccountBalanceUpdated = true; 

            if (CanProcessWithdrawal(cancellationToken))
            {
                var message = new WithdrawalApproved(Guid.NewGuid(), context.Message.CorrelationId);
                Publish(message);
            }
        }
     
        public async Task HandleAsync(IMessageContext<WithdrawalApproved> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"4. Withdrawal Approved (Cash has been sent to ATM): {context.Message.CorrelationId} & Account Number: {State.AccountNumber}");
            this.State.isWithdrawalApproved = true;

            if (State.Amount > 10000)
                throw new ApplicationException("ATM does not have enough cash");
                        
            State.isSuccessful = true;
            var message = new WithdrawalCompleted(Guid.NewGuid(), context.Message.CorrelationId);
            Publish(message);            
        }

        public async Task CompensateAsync(ICompensationContext<WithdrawalApproved> context, CancellationToken cancellationToken = default)
        {
            State.isSuccessful = false;
            Console.WriteLine($"Withdrawal Processing Failed need to refund account balance: {State.AccountNumber} with amount: {State.Amount}");
            var message = new WithdrawalCompleted(Guid.NewGuid(), context.MessageContext.Message.CorrelationId);
            Publish(message);
        }

        public Task HandleAsync(IMessageContext<WithdrawalCompleted> context, CancellationToken cancellationToken = default)
        {
            State.isWithdrawalComplete = true;
            if (State.isSuccessful)
            {
                Console.WriteLine($"Withdrawal Completed Successfully: {context.Message.CorrelationId} & Account Number: {State.AccountNumber}  & Amount: {State.Amount} ");                
            }
            else
            {
                Console.WriteLine($"Withdrawal Completed Failure: {context.Message.CorrelationId} & Account Number: {State.AccountNumber} & Amount: {State.Amount} ");
            }
            
            State.MarkAsCompleted();
            return Task.CompletedTask;
        }

        private bool CanProcessWithdrawal(CancellationToken cancellationToken = default)
        {
            return State.isAccountBalanceUpdated && State.isWithdrawalValidated;
        }
    }
}
