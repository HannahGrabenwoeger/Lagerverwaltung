using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Backend.Data;
using Backend.Services;
using Backend.Services.Firebase;
using Backend.Services.Firestore;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Threading.Tasks;
using Backend.Models;
using Google.Cloud.Firestore.V1;

var builder = WebApplication.CreateBuilder(args);

var firestoreConfig = builder.Configuration.GetSection("Firestore");
var firestorePath = Path.Combine(AppContext.BaseDirectory, firestoreConfig["ServiceAccountPath"] ?? "");
var projectId = firestoreConfig["ProjectId"];

if (!string.IsNullOrEmpty(firestorePath) && File.Exists(firestorePath) && !string.IsNullOrEmpty(projectId))
{
    try
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firestorePath)
            });
        }

        var credential = GoogleCredential.FromFile(firestorePath);
        var builderFirestore = new FirestoreClientBuilder
        {
            Credential = credential
        };
        var client = builderFirestore.Build();
        var firestoreDb = FirestoreDb.Create(projectId, client);
        builder.Services.AddSingleton(firestoreDb);
        builder.Services.AddScoped<IFirestoreDbWrapper, FirestoreDbWrapper>();

        Console.WriteLine("Firestore erfolgreich initialisiert.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Fehler beim Initialisieren von Firestore: " + ex.Message);
    }
}
else
{
    Console.WriteLine("Firestore NICHT initialisiert. Prüfe appsettings.json und Datei-Pfad.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<InventoryReportService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddSingleton<RestockProcessor>();
builder.Services.AddSingleton<IFirebaseAuthWrapper, FirebaseAuthWrapper>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RestockProcessor>());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(warnings => warnings.Ignore(
               Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
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

// === Seed-Daten in DB schreiben ===
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedDataAsync(dbContext);
}

app.Run();

// === SEED ===
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