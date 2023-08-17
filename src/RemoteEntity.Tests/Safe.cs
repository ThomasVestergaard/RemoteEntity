using System;

namespace RemoteEntity.Tests;

public static class Safe
{
    public static void Try(Action action)
    {
        try
        {
            action();
        }
        catch
        {
            // ignored
        }
    }
}