using Microsoft.Extensions.Options;

namespace SharpBot;

public class MotionConfig<T>/* where T : MotionConfig<T>*/
{
    private static IOptionsMonitor<T> motion;
    public static T CurrentValue => motion.CurrentValue;

    public static IDisposable? OnChange(Action<T, string?> listener)
    {
        return motion.OnChange(listener);
    }
}