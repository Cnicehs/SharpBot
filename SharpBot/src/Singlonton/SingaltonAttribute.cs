using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;


public class BaseSgArribute : Attribute
{
}

public class RunningOnAwakeAttribute : BaseSgArribute
{
    public void Invoke(ISingaltonAttribute singal, MethodInfo methodInfo)
    {
        methodInfo.Invoke(singal, new object?[] { });
    }
}