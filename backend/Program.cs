using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT Key is not configured.");
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = authSigningKey
        };
    });

builder.Services.AddScoped<InventoryReportService>();

// 📌 Zugriffskontrolle mit rollenbasierter Autorisierung
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireWarehouseManager", policy => policy.RequireRole("WarehouseManager"));
});

// 📌 Services hinzufügen
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddHostedService<RestockProcessor>();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) => true);  // Fügt alle Controller zu Swagger hinzu
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
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

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await SeedDataAsync(dbContext, userManager, roleManager);
}

app.Run();

async Task SeedDataAsync(AppDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
{
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

        await dbContext.SaveChangesAsync();
        Console.WriteLine("✅ Seed-Daten erfolgreich hinzugefügt!");
    }

    string[] roles = { "BossAdmin", "EmployeeAdmin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    string bossAdminEmail = "bossadmin@example.com";
    if (await userManager.FindByEmailAsync(bossAdminEmail) == null)
    {
        var bossAdmin = new ApplicationUser
        {
            UserName = bossAdminEmail,
            Email = bossAdminEmail,
            FullName = "Boss Admin"  
        };
        var result = await userManager.CreateAsync(bossAdmin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(bossAdmin, "BossAdmin");
        }
    }

    string employeeAdminEmail = "employeeadmin@example.com";
    if (await userManager.FindByEmailAsync(employeeAdminEmail) == null)
    {
        var employeeAdmin = new ApplicationUser
        {
            UserName = employeeAdminEmail,
            Email = employeeAdminEmail,
            FullName = "Employee Admin"  
        };
        var result = await userManager.CreateAsync(employeeAdmin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(employeeAdmin, "EmployeeAdmin");
        }
    }
}