using OpenSleigh.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bank.Messages
{
    public class WithdrawalMessage : SagaState
    {
        public string _accountNumber { get; set; }
        public double _amount { get; set; }
        public WithdrawalMessage(Guid id, string accountnumber, double amount)
            : base(id)
        {
            _accountNumber = accountnumber;
            _amount = amount;
        }
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
    }
}
