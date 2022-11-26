using Microsoft.Extensions.Configuration;
using Serilog;

namespace SharpBot;

public static class Log
{
    public static Serilog.ILogger Logger => Serilog.Log.Logger;
    static Log()
    {
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();
        }
        
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);
        
        
        Serilog.Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Build()).CreateLogger();
        // Serilog.Log.Logger = new LoggerConfiguration().WriteTo.File().CreateLogger();
    }

    public static void Debug(object message)
    {
        Serilog.Log.Debug(message.ToString());
    }

    public static void Debug(string format, params object[] args)
    {
        Serilog.Log.Debug(format, args);
    }

    public static void Warn(object message)
    {
        Serilog.Log.Warning(message.ToString());
    }
    
    public static void Warn(string format, params object[] args)
    {
        Serilog.Log.Warning(format, args);
    }

    public static void Error(object message)
    {
        Serilog.Log.Error(message.ToString());
    }
    
    public static void Error(string format, params object[] args)
    {
        Serilog.Log.Error(format, args);
    }
}