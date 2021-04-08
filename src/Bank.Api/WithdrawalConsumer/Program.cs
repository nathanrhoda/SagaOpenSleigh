using Bank.Saga.AtmWithdrawal;
using Bank.Saga.Withdrawal;
using Microsoft.Extensions.Hosting;
using OpenSleigh.Core.DependencyInjection;
using OpenSleigh.Persistence.Mongo;
using OpenSleigh.Transport.RabbitMQ;
using System;
using System.Threading.Tasks;

namespace WithdrawalConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);
            var host = hostBuilder.Build();

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {

                services.AddOpenSleigh(cfg =>
                {
                    var rabbitSection = hostContext.Configuration.GetSection("Rabbit");
                    var rabbitCfg = new RabbitConfiguration(rabbitSection["HostName"],
                        rabbitSection["UserName"],
                        rabbitSection["Password"]);

                    var mongoSection = hostContext.Configuration.GetSection("Mongo");
                    var mongoCfg = new MongoConfiguration(mongoSection["ConnectionString"],
                        mongoSection["DbName"],
                        MongoSagaStateRepositoryOptions.Default,
                        MongoOutboxRepositoryOptions.Default);

                    cfg.UseRabbitMQTransport(rabbitCfg, builder =>
                    {
                        //builder.UseMessageNamingPolicy<WithdrawalProcessed>(() => new QueueReferences("withdrawal.processed", "withdrawal.processed.start", "withdrawal.processed.dead", "withdrawal.processed.dead.start"));
                    })
                    .UseMongoPersistence(mongoCfg);

                    cfg.AddSaga<AtmWithdrawalSaga, AtmWithdrawalSagaState>()
                      .UseStateFactory<ProcessAtmWithdrawal>(msg => new AtmWithdrawalSagaState(msg.CorrelationId, msg.Amount))
                      .UseRabbitMQTransport();

                });
            });
    }
}
