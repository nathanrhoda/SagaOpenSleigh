using Bank.Messages;
using Bank.Saga;
using Bank.Saga.Withdrawal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenSleigh.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtmController : ControllerBase
    {
        private readonly IMessageBus _bus;
        public AtmController(IMessageBus bus)
        {
            _bus = bus;
        }

        [HttpPost]
        public async Task<ActionResult> Withdrawal(WithdrawalRequest request, CancellationToken cancellationToken = default)
        {
            IMessage message = new WithdrawalInitiated(Guid.NewGuid(), Guid.NewGuid(), request.AccountNumber, request.Amount);
            await _bus.PublishAsync(message, cancellationToken);

            return Accepted(new 
            {
                SagaId = message.CorrelationId
            });
        }
    }
}
