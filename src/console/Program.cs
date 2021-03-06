using System;

[AttributeUsage(AttributeTargets.Method)]
sealed class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type t) { }
}

public class Test
{
    public static int Main(string[] args)
    {
        Console.WriteLine($"Hello from .Net6!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        return args.Length;
    }
}