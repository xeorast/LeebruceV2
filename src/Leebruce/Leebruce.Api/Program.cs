global using Leebruce.Api.Exeptions;
global using Leebruce.Api.Services;
global using Leebruce.Api.Services.LbAuth;
global using Leebruce.Api.Services.LbPages;
using Leebruce.Api;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.
Startup.ConfigureServices( builder );

var app = builder.Build();

// Configure the HTTP request pipeline.
Startup.Configure( app );

app.Run();
