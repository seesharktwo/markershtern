using Facade.GrpcServices;
using Facade.Services;
using Facade.Сonfigs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facade
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(); 

            // configs
            services.Configure<ConnectionString<OrderService>>
                (c => c.String = Configuration.GetValue<string>("ConnectionOrderMicroservice"));
            services.Configure<ConnectionString<UserBriefcaseService>>
                (c => c.String = Configuration.GetValue<string>("ConnectionUserBrifcaseMicroservice"));
            services.Configure<ConnectionString<GetUserID>>
                (c => c.String = Configuration.GetValue<string>("ConnectionAuthorizationMicroservice"));
            services.Configure<ConnectionString<GetListTradeProducts>>
                (c => c.String = Configuration.GetValue<string>("ConnectionProductMicroservice"));


            services.AddTransient<OrderService>();
            services.AddTransient<UserBriefcaseService>();
            services.AddTransient<GetListTradeProducts>();
            services.AddTransient<GetUserID>();

           
          
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapGrpcService<GrpcOrderService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
