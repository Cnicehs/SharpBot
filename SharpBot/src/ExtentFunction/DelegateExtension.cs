using System.Collections;
using System.Collections.Generic;
using System;
public static class DelegateExtension
{
    public static bool IsValid(this Delegate d)
    {
        return d != null && (d.Target != null || d.Method.IsStatic);
    }
    public static bool IsInvalid(this Delegate d)
    {
        return !d.IsValid();
    }
}
