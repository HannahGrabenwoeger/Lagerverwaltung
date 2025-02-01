using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class RestockProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RestockProcessor> _logger;

        public RestockProcessor(IServiceScopeFactory scopeFactory, ILogger<RestockProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var pendingOrders = await context.RestockQueue
                        .Where(r => !r.Processed)
                        .ToListAsync();

                    foreach (var order in pendingOrders)
                    {
                        var product = await context.Products.FindAsync(order.ProductId);
                        if (product != null)
                        {
                            product.Quantity += order.Quantity;
                            order.Processed = true;
                            _logger.LogInformation($"Nachbestellung für {product.Name} verarbeitet.");
                        }
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Alle 5 Minuten prüfen
            }
        }
    }
}