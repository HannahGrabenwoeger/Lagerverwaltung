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
                    var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                    var pendingOrders = await context.RestockQueue
                        .Where(r => !r.Processed)
                        .ToListAsync();

                    foreach (var order in pendingOrders)
                    {
                        var product = await context.Products.FindAsync(order.ProductId);
                        if (product == null)
                        {
                            _logger.LogWarning($"Product with ID {order.ProductId} not found");
                            continue;
                        }

                        product.Quantity += order.Quantity;
                        order.Processed = true;

                        try
                        {
                            await context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            _logger.LogWarning($"Concurrency Exception bei {product.Name}: {ex.Message}");
                            continue; 
                        }

                        if (product.Quantity < product.MinimumStock && !product.HasSentLowStockNotification)
                        {
                            string subject = $"Critical inventory at {product.Name}";
                            int deficit = product.MinimumStock - product.Quantity;
                            string body = $"The current stock of {product.Name} is {product.Quantity} pieces (minimum stock: {product.MinimumStock}). Missing quantity: {deficit} pieces";
                            var managers = await GetManagersAsync(context);

                            foreach (var managerEmail in managers)
                            {
                                await emailService.SendEmailAsync(managerEmail, subject, body);
                                _logger.LogInformation($"Email notification sent to manager {managerEmail} (product: {product.Name})");
                            }
                            product.HasSentLowStockNotification = true;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        public async Task ProcessRestockAsync(Guid productId)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                var product = await context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {productId} not found.");
                    return;
                }

                if (product.Quantity < product.MinimumStock && !product.HasSentLowStockNotification)
                {
                    string subject = $"KCritical stock at {product.Name}";
                    int deficit = product.MinimumStock - product.Quantity;
                    string body = $"The current stock of {product.Name} is {product.Quantity} pieces (minimum stock: {product.MinimumStock}). Missing quantity: {deficit} pieces";
                    var managers = await GetManagersAsync(context);

                    foreach (var managerEmail in managers)
                    {
                        await emailService.SendEmailAsync(managerEmail, subject, body);
                        _logger.LogInformation($"Email notification sent to manager {managerEmail}");
                    }
                    product.HasSentLowStockNotification = true;
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task<List<string>> GetManagersAsync(AppDbContext context)
        {
            return await context.UserRoles
                .Where(ur => ur.Role == "Manager")
                .Select(ur => ur.FirebaseUid) 
                .ToListAsync();
        }
    }
}