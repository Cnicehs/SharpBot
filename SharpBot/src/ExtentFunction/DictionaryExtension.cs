namespace SharpBot.ExtentFunction;

public static class DictionaryExtension
{
    public static V Get<K, V>(this Dictionary<K, V> d, K k)
    {
        if (d.TryGetValue(k, out var value))
        {
            return value;
        }

        return default;
    }

    public static V GetOrDefault<K, V>(this Dictionary<K, V> d, K k, V def = default)
    {
        if (d.TryGetValue(k, out var value))
        {
            return value;
        }

        return def;
    }
}