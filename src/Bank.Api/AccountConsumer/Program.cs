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
                        //builder.UseMessageNamingPolicy<WithdrawalInitiated>(() => new QueueReferences("withdrawal.initiated", "withdrawal.initiated.start", "withdrawal.initiated.dead", "withdrawal.initiated.dead.start"));
                        //builder.UseMessageNamingPolicy<WithdrawalApproved>(() => new QueueReferences("withdrawal.approved", "withdrawal.approved.start", "withdrawal.approved.dead", "withdrawal.approved.dead.start"));
                        //builder.UseMessageNamingPolicy<AccountBalanceUpdated>(() => new QueueReferences("withdrawal.balanceupdate", "withdrawal.balanceupdate.start", "withdrawal.balanceupdate.dead", "withdrawal.balanceupdate.dead.start"));                        
                        //builder.UseMessageNamingPolicy<WithdrawalCompleted>(() => new QueueReferences("withdrawal.completed", "withdrawal.completed.start", "withdrawal.completed.dead", "withdrawal.completed.dead.start"));
                    })
                    .UseMongoPersistence(mongoCfg);

                    cfg.AddSaga<WithdrawalSaga, WithdrawalSagaState>()
                    .UseStateFactory<WithdrawalInitiated>(msg => new WithdrawalSagaState(msg.CorrelationId))                    
                    .UseRabbitMQTransport();

                    //cfg.AddSaga<WithdrawalSaga, WithdrawalSagaState>()
                    //.UseStateFactory<WithdrawalCompleted>(msg => new WithdrawalSagaState(msg.CorrelationId))
                    //.UseRabbitMQTransport();
                });
            });
    }
}
