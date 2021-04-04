﻿using Bank.Saga;
using Bank.Saga.Withdrawal;
using Microsoft.Extensions.Hosting;
using OpenSleigh.Core.DependencyInjection;
using OpenSleigh.Persistence.Mongo;
using OpenSleigh.Transport.RabbitMQ;
using System;
using System.Threading.Tasks;

namespace AccountConsumer
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
                        // using a custom naming policy allows us to define the names for exchanges and queues.
                        // this allows us to have a single exchange bound to multiple queues.
                        // messages will be routed using the queue name.
                        ////builder.UseMessageNamingPolicy<StartChildSaga>(() => new QueueReferences("child", "child.start", "child.dead", "child.dead.start"));
                        ////builder.UseMessageNamingPolicy<ProcessChildSaga>(() => new QueueReferences("child", "child.process", "child.dead", "child.dead.process"));
                    })
                    .UseMongoPersistence(mongoCfg);

                    cfg.AddSaga<WithdrawalSaga, WithdrawalSagaState>()
                    .UseStateFactory<WithdrawalInitiated>(msg => new WithdrawalSagaState(msg.CorrelationId))                    
                    .UseRabbitMQTransport();
                    
                    ////cfg.AddSaga<SimpleSaga, SimpleSagaState>()
                    ////    .UseStateFactory<StartSimpleSaga>(msg => new SimpleSagaState(msg.CorrelationId))
                    ////    .UseRabbitMQTransport();

                    ////cfg.AddSaga<ParentSaga, ParentSagaState>()
                    ////    .UseStateFactory<StartParentSaga>(msg => new ParentSagaState(msg.CorrelationId))
                    ////    .UseRabbitMQTransport();

                    ////cfg.AddSaga<ChildSaga, ChildSagaState>()
                    ////    .UseStateFactory<StartChildSaga>(msg => new ChildSagaState(msg.CorrelationId))
                    ////    .UseRabbitMQTransport();
                });
            });
    }
}
