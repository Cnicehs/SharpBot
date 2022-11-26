using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SharpBot;

public interface ISingaltonAttribute
{
}

internal static class ISingaltonExtension
{
    public static void InitAttribute(this ISingaltonAttribute ins)
    {
        //Running Awake
        foreach (var method in ins.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                        BindingFlags.Public | BindingFlags.NonPublic |
                                                        BindingFlags.FlattenHierarchy))
        {
            method.GetCustomAttributes<RunningOnAwakeAttribute>(true).ForEach((x) =>
            {
                try
                {
                    x.Invoke(ins, method);
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex);
                }
            });
        }
    }
}