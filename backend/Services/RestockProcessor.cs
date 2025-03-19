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
using Microsoft.AspNetCore.Identity;

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
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var pendingOrders = await context.RestockQueue
                        .Where(r => !r.Processed)
                        .ToListAsync();

                    foreach (var order in pendingOrders)
                    {
                        var product = await context.Products.FindAsync(order.ProductId);
                        if (product == null)
                        {
                            _logger.LogWarning($"Produkt mit ID {order.ProductId} nicht gefunden.");
                            continue;
                        }

                        product.Quantity += order.Quantity;
                        order.Processed = true;

                        // 2) Speichern und Concurrency-Fehler abfangen
                        try
                        {
                            await context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            // Bei Concurrency-Fehlern wird kein weiterer Versuch unternommen
                            // => Der neue Bestand ist NICHT gespeichert
                            _logger.LogWarning($"Concurrency Exception bei {product.Name}: {ex.Message}");
                            continue; 
                        }

                        // 3) Nur wenn Speichern geklappt hat, prüfen wir den Mindestbestand
                        if (product.Quantity < product.MinimumStock && !product.HasSentLowStockNotification)
                        {
                            string subject = $"Kritischer Bestand bei {product.Name}";
                            int deficit = product.MinimumStock - product.Quantity;
                            string body = $"Der aktuelle Bestand von {product.Name} beträgt {product.Quantity} Stück (Mindestbestand: {product.MinimumStock}). Fehlende Menge: {deficit} Stück.";
                            var managers = await userManager.GetUsersInRoleAsync("Manager");
                            foreach (var manager in managers)
                            {
                                await emailService.SendEmailAsync(manager.Email!, subject, body);
                                _logger.LogInformation($"E-Mail-Benachrichtigung an Manager {manager.Email} gesendet (Produkt: {product.Name}).");
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
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var product = await context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {productId} not found.");
                    return;
                }

                if (product.Quantity < product.MinimumStock && !product.HasSentLowStockNotification)
                {
                    string subject = $"Kritischer Bestand bei {product.Name}";
                    int deficit = product.MinimumStock - product.Quantity;
                    string body = $"Der aktuelle Bestand von {product.Name} beträgt {product.Quantity} Stück (Mindestbestand: {product.MinimumStock}). Fehlende Menge: {deficit} Stück.";
                    var managers = await userManager.GetUsersInRoleAsync("Manager");
                    foreach (var manager in managers)
                    {
                        await emailService.SendEmailAsync(manager.Email!, subject, body);
                        _logger.LogInformation($"E-Mail-Benachrichtigung an Manager {manager.Email} gesendet.");
                    }
                    product.HasSentLowStockNotification = true;
                }

                await context.SaveChangesAsync();
            }
        }
    }
}