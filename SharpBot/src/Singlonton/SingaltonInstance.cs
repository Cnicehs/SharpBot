using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class SingaltonInstance<T> : ISingaltonAttribute where T : SingaltonInstance<T>, new()
{
    private static T instance;

    public static T Instance => GetInstance();

    private static T GetInstance()
    {
        if (instance == null)
        {
            lock (typeof(T))
            {
                if (instance == null)
                {
                    instance = new T();
                    instance.InitAttribute();
                }
            }
        }

        return instance;
    }

    protected SingaltonInstance()
    {
    }
}