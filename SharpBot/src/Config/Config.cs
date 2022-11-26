using System.Reflection;
using SharpBot.ExtentFunction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace SharpBot;

public class Config : SingaltonInstance<Config>
{
    public void Init(WebApplicationBuilder builder)
    {
        var yaml = File.ReadAllText("./config/config.yaml");
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
        var result = deserializer.Deserialize<Dictionary<object, object>>(yaml);
        var envs = System.Environment.GetEnvironmentVariables();
        foreach (string key in envs.Keys)
        {
            if (key.StartsWith("SharpBot_"))
            {
                var paths = key.Split("_").ToList();
                paths.RemoveAt(0);
                var now = result;
                for (int i = 0; i < paths.Count; i++)
                {
                    if (!now.ContainsKey(paths[i]))
                    {
                        break;
                    }

                    if (!(now is Dictionary<object, object>))
                    {
                        break;
                    }

                    if (i != paths.Count - 1)
                    {
                        now = now[paths[i]] as Dictionary<object, object>;
                    }
                    else
                    {
                        if (now[paths[i]].ToString() == "")
                        {
                            now[paths[i]] = envs[key];
                        }
                    }
                }
            }
        }

        var s = new YamlDotNet.Serialization.SerializerBuilder().Build();
        File.WriteAllText("./config/config.yaml", s.Serialize(result));

        builder.Configuration.AddYamlFile("config/config.yaml", true, true);
        var server = builder.Services;
        var method = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(x => x.Name == "Configure" && x.GetParameters().Length == 2);
        Assembly.GetExecutingAssembly().GetTypes().ForEach(type =>
        {
            if (type.GetAllAttribute<ConfigAttribute>().Count > 0)
            {
                method.MakeGenericMethod(type)
                    .Invoke(null, new object[] { server, builder.Configuration.GetSection(type.Name) });
            }
        });
    }
}