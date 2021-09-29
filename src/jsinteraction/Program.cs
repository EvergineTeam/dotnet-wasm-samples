using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

public class Program
{
    private static WebAssemblyRuntime? wasm;

    public static void Main()
    {
        wasm = WebAssemblyRuntime.GetInstance();
        Console.WriteLine("Wasm Ready!");

        var box = wasm.GetElementById("box");
        var style = box?.GetObjectProperty<JSObject>("style");
        style?.SetObjectProperty("backgroundColor", "green");

        box?.AddSimpleEventListener("click", new Action(OnBoxClick), false);
    }

    public static void OnBoxClick()
    {
        wasm?.Invoke("alert", warn: true, "hello!");
    }
}