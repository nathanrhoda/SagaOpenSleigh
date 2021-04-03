using Bank.Messages;
using OpenSleigh.Core;
using System;

namespace Bank.Saga
{
    public class WithdrawalInitiatedState : SagaState
    {
        public WithdrawalMessage _message { get; set; }
        public WithdrawalInitiatedState(WithdrawalMessage message) 
            : base(message.Id)
        {
            _message = message;
        }
    }
}
