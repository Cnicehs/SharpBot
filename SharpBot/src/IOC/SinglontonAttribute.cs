namespace SharpBot.IOC;

using System.Reflection;

[AttributeUsage(AttributeTargets.Class)]
public class SinglontonAttribute : Attribute
{
    public bool RunningOnAwake = false;
    public int Order = 0;
}

public static class SingletonsExt
{
    public static IServiceCollection AddSinglontons(this WebApplicationBuilder builder)
    {
        return builder.Services.AddSinglontons();
    }

    public static IServiceCollection AddSinglontons(this IServiceCollection server)
    {
        var method = typeof(ServiceCollectionServiceExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(x => x.Name == "AddSingleton" && x.GetParameters().Length == 1 && x.IsGenericMethod &&
                        x.GetGenericArguments().Length == 1);

        //builder.Services.AddSingleton<TimeServer>(x=>x.GetServices<AddressFamily>())
        var methodForward = typeof(ServiceCollectionServiceExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(x => x.Name == "AddSingleton" && x.GetParameters().Length == 3 && !x.IsGenericMethod &&
                        x.GetParameters().Last().ParameterType.IsGenericType &&
                        x.GetParameters().Last().ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        Assembly.GetExecutingAssembly().GetTypes().ForEach(type =>
        {
            if (type.GetAllAttribute<SinglontonAttribute>().Count > 0)
            {
                method.MakeGenericMethod(type)
                    .Invoke(null, new object[] { server });

                type.GetInterfaces().ForEach(i =>
                {
                    methodForward.Invoke(null,
                        new object[] { server, i, (IServiceProvider provide) => provide.GetService(type) });
                });
            }
        });
        return server;
    }

    public static WebApplication Setup(this WebApplication server)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        List<(Type, int)> toAwakes = new List<(Type, int)>();
        assembly.GetTypes().ForEach(type =>
        {
            var attribute = type.GetCustomAttribute<SinglontonAttribute>(false);
            if (attribute != null && attribute.RunningOnAwake)
            {
                toAwakes.Add((type, attribute.Order));
            }
        });

        toAwakes.Sort((x, y) =>
        {
            if (x.Item2 != y.Item2)
            {
                return x.Item2 - y.Item2;
            }
            
            return String.CompareOrdinal(x.ToString(), y.ToString());
        });
        
        toAwakes.ForEach(x =>
        {
            server.Services.GetService(x.Item1);
        });

        return server;
    }
}