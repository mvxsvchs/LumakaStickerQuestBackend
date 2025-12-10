using LumakaStickerQuestBackend;
using LumakaStickerQuestBackend.Functions;
using Npgsql;

// Get connection string
//string connectionString = ConfigurationHelper.GetConnectionString("DefaultConnection");

// Connect to the PostgreSQL server
//await using var conn = new NpgsqlConnection(connectionString);
//await conn.OpenAsync();

//Console.WriteLine($"PostgreSQL version: {conn.PostgreSqlVersion}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Services>();
builder.Services.AddScoped<Services.UserS>();
builder.Services.AddScoped<Services.ListS>();
builder.Services.AddScoped<Services.BoardS>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
