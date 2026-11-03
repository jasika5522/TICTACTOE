using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using TICTACTOEGATEWAY;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<BypassSslValidationHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase) ||
                origin.StartsWith("https://localhost:", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5232",
                "https://localhost:7229");
        }
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");

app.Use(async (context, next) =>
{
    if (HttpMethods.Options.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }
    await next();
});

app.UseHttpsRedirection();

await app.UseOcelot();

app.Run();
