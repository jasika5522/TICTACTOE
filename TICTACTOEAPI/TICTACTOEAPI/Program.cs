using Dapper;
using TICTACTOEAPI.BAL.Implementations;
using TICTACTOEAPI.BAL.Interfaces;
using TICTACTOEAPI.DAL.DbConnection;
using TICTACTOEAPI.DAL.Implementations;
using TICTACTOEAPI.DAL.Interfaces;
using TICTACTOEAPI.DAL.TypeHandlers;


// Register Dapper type handlers
SqlMapper.AddTypeHandler(new CharTypeHandler());
SqlMapper.AddTypeHandler(new NullableCharTypeHandler());

// Match snake_case columns to PascalCase properties
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAL
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IScoreboardRepository, ScoreboardRepository>();


// BAL
builder.Services.AddScoped<IGameLogicService, GameLogicService>();
builder.Services.AddScoped<IGameService, GameService>();

// CORS for Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();