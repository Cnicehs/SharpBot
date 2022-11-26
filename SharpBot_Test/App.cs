using SharpBot.IOC;
using SharpBot.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SharpBot_Test;

public class App
{
    private WebApplication app;

    public App()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddConfig();
        builder.AddSinglontons();
        this.app = builder.Build();
    }
    
    public T GetService<T>()
    {
        return app.Services.GetService<T>();
    }
}