var builder = WebApplication.CreateBuilder(args);

// Swagger-Dienste hinzufügen
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controller-Dienste hinzufügen
builder.Services.AddControllers();

var app = builder.Build();

// Swagger aktivieren (nur in Entwicklungsumgebung)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware hinzufügen
app.UseAuthorization();

app.MapControllers();

app.Run();