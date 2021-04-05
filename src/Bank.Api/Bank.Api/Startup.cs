using Bank.Saga.Withdrawal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenSleigh.Core.DependencyInjection;
using OpenSleigh.Persistence.Mongo;
using OpenSleigh.Transport.RabbitMQ;

// TODO: Have more than 1 consumer for the Saga & return a value 
// TODO: Integrate grpc with the queueing tasks
// TODO: Create a UI that will send a request and wait for positive response
// TODO: Implement negative scenario from UI as well

namespace Bank.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bank.Api", Version = "v1" });
            });

            services.AddOpenSleigh(cfg =>
            {
                var mongoSection = Configuration.GetSection("Mongo");
                var mongoCfg = new MongoConfiguration(mongoSection["ConnectionString"],
                    mongoSection["DbName"]);


                var rabbitSection = Configuration.GetSection("Rabbit");
                var rabbitCfg = new RabbitConfiguration(rabbitSection["HostName"],
                    rabbitSection["UserName"],
                    rabbitSection["Password"]);

                cfg.SetPublishOnly()
                    .UseRabbitMQTransport(rabbitCfg, builder =>
                    {
                        //builder.UseMessageNamingPolicy<WithdrawalInitiated>(() => new QueueReferences("withdrawal.initiated", "withdrawal.initiated.start", "withdrawal.initiated.dead", "withdrawal.initiated.dead.start"));
                    })
                    .UseMongoPersistence(mongoCfg);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
