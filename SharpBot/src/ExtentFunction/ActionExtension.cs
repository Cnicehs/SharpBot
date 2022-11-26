namespace SharpBot.ExtentFunction;

public static class ActionExtension
{
    public static void Pipe<T>(this T t, Action<T> act)
    {
        act.Invoke(t);
    }

    public static K Pipe<T, K>(this T t, Func<T, K> act)
    {
        return act.Invoke(t);
    }
}