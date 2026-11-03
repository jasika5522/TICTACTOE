using Dapper;
using PortfolioService.BAL.Implementations;
using PortfolioService.BAL.Interfaces;
using PortfolioService.DAL.DbConnection;
using PortfolioService.DAL.Implementations;
using PortfolioService.DAL.Interfaces;

DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5232",
                "https://localhost:7229",
                "https://localhost:7000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowClients");
app.UseAuthorization();
app.MapControllers();

app.Run();
