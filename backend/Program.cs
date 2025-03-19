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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireWarehouseManager", policy => policy.RequireRole("WarehouseManager"));
});

builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<UserQueryService>();
builder.Services.AddSingleton<RestockProcessor>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RestockProcessor>());

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) => true);  
});

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

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "";  
});

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
    if (!await roleManager.RoleExistsAsync("Manager"))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>("Manager"));
    }
    
    if (!await roleManager.RoleExistsAsync("Employee"))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>("Employee"));
    }

    if (!dbContext.Warehouses.Any())
    {
        dbContext.Warehouses.AddRange(
            new Warehouse { Id = Guid.NewGuid(), Name = "Lager A", Location = "Standort A" },
            new Warehouse { Id = Guid.NewGuid(), Name = "Lager B", Location = "Standort B" }
        );

       dbContext.Warehouses.AddRange(
            new Warehouse { Id = Guid.NewGuid(), Name = "Lager A", Location = "Standort A" },
            new Warehouse { Id = Guid.NewGuid(), Name = "Lager B", Location = "Standort B" }
        );

        await dbContext.SaveChangesAsync();
        Console.WriteLine("Seed-Daten für Warehouses und Produkte hinzugefügt!");
    }

    string managerEmail = "manager@example.com";
    if (await userManager.FindByEmailAsync(managerEmail) == null)
    {
        var manager = new ApplicationUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            FullName = "Manager"  
        };
        var result = await userManager.CreateAsync(manager, "ManagerPassword123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(manager, "Manager");
        }
    }

    string employeeEmail = "employee@example.com";
    if (await userManager.FindByEmailAsync(employeeEmail) == null)
    {
        var employee = new ApplicationUser
        {
            UserName = employeeEmail,
            Email = employeeEmail,
            FullName = "Employee"  
        };
        var result = await userManager.CreateAsync(employee, "EmployeePassword123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(employee, "Employee");
        }
    }

    Console.WriteLine("Seed-Daten für Manager und Employee Benutzer hinzugefügt!");
}