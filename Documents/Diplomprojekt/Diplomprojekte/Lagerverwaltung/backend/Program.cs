using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.OpenApi.Models;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("Secrets/service-account.json")
});

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<InventoryReportService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<UserQueryService>();
builder.Services.AddSingleton<RestockProcessor>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RestockProcessor>());

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) => true);
});

builder.Services.AddSingleton<EmailService>(sp =>
    new EmailService(
        smtpServer: "sandbox.smtp.mailtrap.io",
        smtpPort: 587,
        smtpUser: "77cfd1069e13e9",
        smtpPassword: "25037aeb7aeb51",
        fromAddress: "no-reply@example.com"
    )
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "";
});

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    await SeedDataAsync(dbContext);
}

app.Run();

async Task SeedDataAsync(AppDbContext dbContext)
{
    if (!dbContext.Warehouses.Any())
    {
        dbContext.Warehouses.AddRange(
            new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse A", Location = "Location A" },
            new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse B", Location = "Location B" }
        );
    }

    if (!dbContext.UserRoles.Any())
    {
        dbContext.UserRoles.Add(new UserRole
        {
            FirebaseUid = "firebase-uid-of-manager",
            Role = "Manager"
        });
    }

    await dbContext.SaveChangesAsync();
}