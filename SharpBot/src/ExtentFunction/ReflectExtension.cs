using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class ReflectExtension
{
    public const BindingFlags bindingParent = System.Reflection.BindingFlags.Public |
                                              System.Reflection.BindingFlags.NonPublic |
                                              System.Reflection.BindingFlags.Static |
                                              System.Reflection.BindingFlags.Instance |
                                              System.Reflection.BindingFlags.FlattenHierarchy;

    public const BindingFlags bindingSelf = System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic |
                                            System.Reflection.BindingFlags.Static |
                                            System.Reflection.BindingFlags.Instance |
                                            System.Reflection.BindingFlags.DeclaredOnly;

    public const BindingFlags bindingPublicParent = System.Reflection.BindingFlags.Public |
                                                    System.Reflection.BindingFlags.Static |
                                                    System.Reflection.BindingFlags.Instance |
                                                    System.Reflection.BindingFlags.FlattenHierarchy;

    public const BindingFlags bindingPublicSelf = System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.Static |
                                                  System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.DeclaredOnly;

    public static T GetFirstAttribute<T>(this Type type, BindingFlags flags) where T : Attribute
    {
        var at = type.GetCustomAttribute<T>();
        if (at != null)
            return at;

        foreach (var field in type.GetFields(flags))
        {
            at = field.GetCustomAttribute<T>();
            if (at != null)
                return at;
        }

        foreach (var method in type.GetMethods(flags))
        {
            at = method.GetCustomAttribute<T>();
            if (at != null)
                return at;
        }

        foreach (var property in type.GetProperties(flags))
        {
            at = property.GetCustomAttribute<T>();
            if (at != null)
                return at;
        }

        return null;
    }

    public static List<T> GetAllAttribute<T>(this Type type, BindingFlags flags = bindingSelf) where T : Attribute
    {
        List<T> ret = new List<T>();
        var at = type.GetCustomAttributes<T>();
        if (at != null)
            ret.AddRange(at);

        foreach (var field in type.GetFields(flags))
        {
            at = field.GetCustomAttributes<T>();
            if (at != null)
                ret.AddRange(at);
        }

        foreach (var method in type.GetMethods(flags))
        {
            at = method.GetCustomAttributes<T>();
            if (at != null)
                ret.AddRange(at);
        }

        foreach (var property in type.GetProperties(flags))
        {
            at = property.GetCustomAttributes<T>();
            if (at != null)
                ret.AddRange(at);
        }

        return ret;
    }

    public static List<Attribute> GetAllAttribute(this Type type, Type attribute, BindingFlags flags = bindingSelf)
    {
        List<Attribute> ret = new List<Attribute>();
        var at = type.GetCustomAttributes();
        if (at != null)
            ret.AddRange(at.Where(t => t.GetType().IsSubclassOf(attribute) || (t.GetType() == attribute)));

        foreach (var field in type.GetFields(flags))
        {
            at = field.GetCustomAttributes();
            if (at != null)
                ret.AddRange(at.Where(t => t.GetType().IsSubclassOf(attribute) || (t.GetType() == attribute)));
        }

        foreach (var method in type.GetMethods(flags))
        {
            at = method.GetCustomAttributes();
            if (at != null)
                ret.AddRange(at.Where(t => t.GetType().IsSubclassOf(attribute) || (t.GetType() == attribute)));
        }

        foreach (var property in type.GetProperties(flags))
        {
            at = property.GetCustomAttributes();
            if (at != null)
                ret.AddRange(at.Where(t => t.GetType().IsSubclassOf(attribute) || (t.GetType() == attribute)));
        }

        return ret;
    }

    //关于Flatten无效
    //https://stackoverflow.com/questions/9201859/why-doesnt-type-getfields-return-backing-fields-in-a-base-class
    public static FieldInfo GetFieldInfoInclueBase(this Type type, string name, BindingFlags flags = bindingSelf)
    {
        do
        {
            var field = type.GetField(name, flags);
            if (field != null)
                return field;
            type = type.BaseType;
        } while (type != null);

        return null;
    }

    public static object GetDefaultValue(this Type t)
    {
        if (t.IsValueType)
            return Activator.CreateInstance(t);

        return null;
    }

    public static object GetValueByPath(this object obj, string path, BindingFlags flags = bindingParent)
    {
        foreach (var t in path.Split('.'))
        {
            var filed = obj.GetType().GetField(t, flags);
            if (filed != null)
            {
                obj = filed.GetValue(obj);
            }
            else
            {
                var property = obj.GetType().GetProperty(t, flags);
                if (property != null)
                {
                    obj = property.GetValue(obj);
                }
                else
                {
                    throw new Exception("Can not find " + path);
                }
            }
        }

        return obj;
    }

    public static void SetValueByPath(this object obj, string path, object value, BindingFlags flags = bindingParent)
    {
        var pt = path.Split('.');
        int count = pt.Length;
        for (int i = 0; i < count; i++)
        {
            var t = pt[i];
            var filed = obj.GetType().GetField(t, flags);
            if (filed != null)
            {
                if (i == count - 1)
                {
                    filed.SetValue(obj, value);
                }
                else
                {
                    obj = filed.GetValue(obj);
                }
            }
            else
            {
                var property = obj.GetType().GetProperty(t, flags);
                if (property != null)
                {
                    if (i == count - 1)
                    {
                        property.SetValue(obj, value);
                    }
                    else
                    {
                        obj = property.GetValue(obj);
                    }
                }
                else
                {
                    throw new Exception("Can not find " + path);
                }
            }
        }
    }

    /// <summary>
    /// This Methode extends the System.Type-type to get all extended methods. It searches hereby in all assemblies which are known by the current AppDomain.
    /// </summary>
    /// <remarks>
    /// Insired by Jon Skeet from his answer on http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
    /// </remarks>
    /// <returns>returns MethodInfo[] with the extended Method</returns>
    public static MethodInfo[] GetExtensionMethods(this Type t)
    {
        List<Type> AssTypes = new List<Type>();

        foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
        {
            AssTypes.AddRange(item.GetTypes());
        }

        var query = from type in AssTypes
            where type.IsSealed && !type.IsGenericType && !type.IsNested
            from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            where method.IsDefined(typeof(ExtensionAttribute), false)
            where method.GetParameters()[0].ParameterType == t
            select method;
        return query.ToArray<MethodInfo>();
    }

    /// <summary>
    /// Extends the System.Type-type to search for a given extended MethodeName.
    /// </summary>
    /// <param name="MethodeName">Name of the Methode</param>
    /// <returns>the found Methode or null</returns>
    public static MethodInfo GetExtensionMethod(this Type t, string MethodeName)
    {
        var mi = from methode in t.GetExtensionMethods()
            where methode.Name == MethodeName
            select methode;
        if (mi.Count<MethodInfo>() <= 0)
            return null;
        else
            return mi.First<MethodInfo>();
    }
}