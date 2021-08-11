using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


public class Program
{
    public static int Main(string[] args)
    {

        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Build().RunAsync();

        Console.WriteLine("Wasm Ready!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        return args.Length;
    }

    public static void Run()
    {
        var file = File.ReadAllText("content/file.txt");
        Console.WriteLine(file);
    }
}