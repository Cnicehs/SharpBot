// See https://aka.ms/new-console-template for more information
using SharpBot;
using SharpBot.Service;
using SharpBot.IOC;
using Microsoft.AspNetCore.OpenApi;
using Serilog;
using Log = SharpBot.Log;

var builder = WebApplication.CreateBuilder(args);
builder.AddConfig();
builder.Services.AddLogging(loggerBuilder =>
{
    loggerBuilder.AddSerilog(Log.Logger);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddSinglontons();


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", opt =>
    {
        opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.Setup();
app.UseCors("CorsPolicy");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
// app.MapGet("/", () => "Hello World!").WithName("GetWeatherForecast").WithOpenApi();;

app.Run();