using SharpBot.ExtentFunction;
using Catalyst.Models;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace SharpBot.Service;
using System.Reflection;
public static class AppExt
{
    public static  WebApplicationBuilder AddConfig(this  WebApplicationBuilder app)
    {
        Config.Instance.Init(app);
        return app;
    }
}