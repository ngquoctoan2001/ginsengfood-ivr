using Ivr.Api.Foundation;
using Ivr.Api.Health;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIvrFoundation(builder.Configuration);
builder.Services.AddIvrApiFoundation(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.UseIvrApiFoundation();
app.MapIvrHealthEndpoints();

app.Run();

public partial class Program;
