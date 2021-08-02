// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


public class Test
{
    public static async Task<int> Main(string[] args)
    {

        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Build().RunAsync();

        await Task.Delay(5000);
        Console.WriteLine("Hello World!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        var file = File.ReadAllText("file.txt");
        Console.WriteLine(file);

        return args.Length;
    }
}