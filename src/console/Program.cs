// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Components.WebAssembly.Http;

public class Test
{
    public static async Task<int> Main(string[] args)
    {
        // var t = BrowserRequestCache.ForceCache;

        await Task.Delay(1);
        Console.WriteLine("Hello World!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        Console.WriteLine($"The answer to everything is {NativeBinding.hello()}");

        return args.Length;
    }
}