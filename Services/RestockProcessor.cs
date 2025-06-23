using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

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
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                var pendingOrders = await context.RestockQueue
                    .Where(r => !r.Processed)
                    .ToListAsync(stoppingToken);

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
                        await context.SaveChangesAsync(stoppingToken);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogWarning($"Concurrency exception on {product.Name}: {ex.Message}");
                        continue;
                    }

                    await NotifyIfLowStock(product, context, emailService);
                    await context.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        public async Task ProcessRestockAsync(Guid productId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            var product = await context.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {productId} not found.");
                return;
            }

            await NotifyIfLowStock(product, context, emailService);
            await context.SaveChangesAsync();
        }

        private async Task NotifyIfLowStock(Product product, AppDbContext context, EmailService emailService)
        {
            if (product.Quantity < product.MinimumStock && !product.HasSentLowStockNotification)
            {
                string subject = $"Critical stock at {product.Name}";
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

        private async Task<List<string>> GetManagersAsync(AppDbContext context)
        {
            return await context.UserRoles
                .Where(ur => ur.Role == "manager")
                .Select(ur => ur.FirebaseUid)
                .ToListAsync();
        }
    }
}