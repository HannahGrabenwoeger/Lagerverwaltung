using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Backend.Data;
using Backend.Services;
using Backend.Services.Firebase;
using Backend.Models;
using Backend.Servicesxs;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:80");

var configuration = builder.Configuration;
var testMode = configuration.GetValue<bool>("TestMode");
builder.Services.AddSingleton(new AppSettings { TestMode = testMode });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
);

builder.Services.AddScoped<InventoryReportService>();
builder.Services.AddScoped(sp => new EmailService(
    configuration["Smtp:Server"]!,
    int.Parse(configuration["Smtp:Port"]!),
    configuration["Smtp:User"]!,
    configuration["Smtp:Password"]!,
    configuration["Smtp:FromAddress"]!
));
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddSingleton<IFirebaseAuthWrapper, FirebaseAuthWrapper>();

builder.Services.AddSingleton<RestockProcessor>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RestockProcessor>());

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.WriteIndented = true;
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowFrontend", p =>
    {
        p.WithOrigins("http://localhost:3000")
         .AllowAnyMethod()
         .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lagerverwaltung API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

if (testMode)
{
    Console.WriteLine("TestMode aktiv – Testdaten werden eingefügt");
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedTestDataAsync(dbContext);
}

app.Run();

async Task SeedTestDataAsync(AppDbContext dbContext)
{
    if (!dbContext.Warehouses.Any())
    {
        dbContext.Warehouses.AddRange(
            new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse A", Location = "Location A" },
            new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse B", Location = "Location B" }
        );
    }

     if (!dbContext.UserRoles.Any(u => u.FirebaseUid == "manager"))
    {
        dbContext.UserRoles.Add(new UserRole { FirebaseUid = "manager", Role = "Manager" });
    }
    if (!dbContext.UserRoles.Any(u => u.FirebaseUid == "employee"))
    {
        dbContext.UserRoles.Add(new UserRole { FirebaseUid = "employee", Role = "Employee" });
    }
    await dbContext.SaveChangesAsync();
}