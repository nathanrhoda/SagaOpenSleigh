using Bank.Saga;
using Bank.Saga.Withdrawal;
using Microsoft.Extensions.Hosting;
using OpenSleigh.Core.DependencyInjection;
using OpenSleigh.Persistence.Mongo;
using OpenSleigh.Transport.RabbitMQ;
using System;
using System.Threading.Tasks;

namespace Bank.Orchestrator
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
                   })
                   .UseMongoPersistence(mongoCfg);

                   cfg.AddSaga<WithdrawalSaga, WithdrawalSagaState>()
                   .UseStateFactory<WithdrawalInitiated>(msg => new WithdrawalSagaState(msg.CorrelationId))
                   .UseRabbitMQTransport()
                   .UseRetryPolicy<WithdrawalInitiated>(builder =>
                   {
                       builder.WithMaxRetries(3)
                          .Handle<RetryException>()
                                   .WithDelay(executionIndex => TimeSpan.FromSeconds(executionIndex))
                                   .OnException(ctx =>
                                   {
                                       System.Console.WriteLine(
                                           $"tentative #{ctx.ExecutionIndex} failed: {ctx.Exception.Message}");
                                   });
                   });                  
               });
           });
    }
}
