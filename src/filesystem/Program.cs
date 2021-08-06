using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


public class Program
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

        var file = File.ReadAllText("content/file.txt");
        Console.WriteLine(file);

        return args.Length;
    }
}