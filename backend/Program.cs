using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") 
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend"); // CORS aktivieren

app.UseAuthorization();
app.MapControllers();  

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!dbContext.Warehouses.Any())
    {
        Guid warehouseId1 = Guid.NewGuid();
        Guid warehouseId2 = Guid.NewGuid();

        dbContext.Warehouses.AddRange(
            new Warehouse { Id = warehouseId1, Name = "Lager A", Location = "Standort A" },
            new Warehouse { Id = warehouseId2, Name = "Lager B", Location = "Standort B" }
        );

        dbContext.Products.AddRange(
            new Products { Id = Guid.NewGuid(), Name = "Produkt 1", Quantity = 100, WarehouseId = warehouseId1 },
            new Products { Id = Guid.NewGuid(), Name = "Produkt 2", Quantity = 50, WarehouseId = warehouseId2 }
        );

        dbContext.SaveChanges();

        Console.WriteLine("Seed-Daten erfolgreich hinzugefügt!");
    }
}

app.Run();