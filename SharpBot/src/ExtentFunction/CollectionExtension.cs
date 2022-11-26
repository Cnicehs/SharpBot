using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class CollectionExtension
{
    public static void ForEach<T>(this IEnumerable<T> enumer, Action<T> action)
    {
        foreach (var em in enumer)
        {
            action?.Invoke(em);
        }
    }

    public static async Task ForEachAsync<T, K>(this IEnumerable<T> enumer, Func<T, K> action) where K : Task
    {
        foreach (var em in enumer)
        {
            await action?.Invoke(em);
        }
    }

    public static T RandomChoose<T>(this IList<T> ls)
    {
        int i = System.Random.Shared.Next(0, ls.Count);
        return ls[i];
    }

    public static int IndexOf<T>(this IList<T> ls, Func<T, bool> predicate)
    {
        int count = ls.Count;
        for (var i = 0; i < count; i++)
        {
            if (predicate(ls[i]))
                return i;
        }

        return -1;
    }

    public static bool TryRemove<T>(this ICollection<T> t, T obj)
    {
        return t.Remove(obj);
    }

    public static bool TryAddWithoutRepeat<T>(this ICollection<T> t, T obj)
    {
        if (!t.Contains(obj))
        {
            t.Add(obj);
            return true;
        }

        return false;
    }
}