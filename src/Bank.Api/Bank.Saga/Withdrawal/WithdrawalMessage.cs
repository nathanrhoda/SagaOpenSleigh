using OpenSleigh.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bank.Messages
{
    public class WithdrawalMessage : SagaState
    {        
        public WithdrawalMessage(Guid correlationId, string accountnumber, double amount)
            : base(correlationId)
        {
            AccountNumber = accountnumber;
            Amount = amount;
        }
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
    }
}
