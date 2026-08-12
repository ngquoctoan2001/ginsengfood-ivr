using Ivr.Api.Health;
using Ivr.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIvrFoundation(builder.Configuration);

var app = builder.Build();

app.MapIvrHealthEndpoints();

app.Run();

public partial class Program;
