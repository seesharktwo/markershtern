using Facade.GrpcServices;
using Facade.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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

            
            services.AddGrpcClient<Order.Orders.OrdersClient>(o =>
            {
                o.Address = new Uri(Configuration.GetValue<string>("ConnectionOrderMicroservice"));
            });
            services.AddGrpcClient<Briefcase.UserBriefcaseService.UserBriefcaseServiceClient>(o =>
            {
                o.Address = new Uri(Configuration.GetValue<string>("ConnectionUserBrifcaseMicroservice"));
            });
            services.AddGrpcClient<Product.ProductService.ProductServiceClient>(o =>
            {
                o.Address = new Uri(Configuration.GetValue<string>("ConnectionProductMicroservice"));
            });
            services.AddGrpcClient<Authorization.AuthorizationService.AuthorizationServiceClient>(o =>
            {
                o.Address = new Uri(Configuration.GetValue<string>("ConnectionAuthorizationMicroservice"));
            });


            services.AddTransient<OrderService>();
            services.AddTransient<UserBriefcaseService>();
            services.AddTransient<ProductService>();
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
                endpoints.MapGrpcService<GrpcProductService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
