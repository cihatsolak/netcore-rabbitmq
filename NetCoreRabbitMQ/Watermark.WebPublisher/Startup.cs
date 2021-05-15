using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using Watermark.WebPublisher.BackgroundServices;
using Watermark.WebPublisher.Models;
using Watermark.WebPublisher.Services;

namespace Watermark.WebPublisher
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: "VehicleDb");
            });

            services.AddSingleton(serviceProvier => new ConnectionFactory()
            {
                Uri = new Uri("amqps://wgpkwzxn:m2DXQ3IKLkxBTvLTRIdwZdNk-PPjvpzi@baboon.rmq.cloudamqp.com/wgpkwzxn"),
                DispatchConsumersAsync = true //ConnectionFactory'i asenkron kullandýðýmdan dolayý
            });

            services.AddSingleton<RabbitMQClientService>();
            services.AddSingleton<RabbitMQPublisher>();

            services.AddHostedService<ImageWatermakProcessBackgroundService>();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
