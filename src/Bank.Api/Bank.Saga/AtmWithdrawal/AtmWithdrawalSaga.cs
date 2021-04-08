using Bank.Saga.Withdrawal;
using OpenSleigh.Core;
using OpenSleigh.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bank.Saga.AtmWithdrawal
{
    public class AtmWithdrawalSagaState : SagaState
    {
        public AtmWithdrawalSagaState(Guid Id, double amount)
            : base(Id)
        {                                        
        }
        
        public double Amount { get; set; }

        public bool IsSuccessful { get; set; }
    }

    public record ProcessAtmWithdrawal(Guid Id, Guid CorrelationId, double Amount) : IMessage { }

    public class AtmWithdrawalSaga :
        Saga<AtmWithdrawalSagaState>,
        IStartedBy<ProcessAtmWithdrawal>
    {
        public async Task HandleAsync(IMessageContext<ProcessAtmWithdrawal> context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"4. Withdrawal Processing (Cash has been sent to ATM): {context.Message.CorrelationId}");

            State.IsSuccessful = true;
            if (context.Message.Amount > 10000)
            {
                State.IsSuccessful = false;
                Console.WriteLine($"4. Withdrawal Processing Failure not enough cash in ATM: {context.Message.CorrelationId}");
            }

            
            State.MarkAsCompleted();
            var message = new WithdrawalCompleted(Guid.NewGuid(), context.Message.CorrelationId);
            Publish(message);
            
        }
    }
}
