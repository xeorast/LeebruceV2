global using Leebruce.Api.Services;
global using Leebruce.Api.Services.LbAuth;
global using Leebruce.Api.Exeptions;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

_ = builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
_ = builder.Services.AddEndpointsApiExplorer();
_ = builder.Services.AddSwaggerGen();
_ = builder.Services.AddScoped<ILbLoginService, LbLoginService>();
_ = builder.Services.AddScoped<ILbLogoutService, LbLogoutService>();
_ = builder.Services.AddScoped<JsonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
	_ = app.UseSwagger();
	_ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
