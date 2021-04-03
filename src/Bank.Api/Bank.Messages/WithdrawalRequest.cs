using System;
using System.Collections.Generic;
using System.Text;

namespace Bank.Messages
{
    public class WithdrawalRequest
    {        
        public WithdrawalRequest()            
        {            
        }
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
    }
}
