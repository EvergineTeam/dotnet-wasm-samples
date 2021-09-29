using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;

/// <summary>
/// Class to encapsulate web assembly operations.
/// </summary>
public class WebAssemblyRuntime : IDisposable
{
    private static WebAssemblyRuntime? instance = null;
    private DotNetObjectReference<WebAssemblyRuntime>? dotnetRefrence = null;
    private Action<double>? requestAnimationFrameAction = null;

    /// <summary>
    /// Gets the runtime for interact with JavaScript.
    /// </summary>
    public WebAssemblyJSRuntime Runtime { get; private set; }

    /// <summary>
    /// Gets or creates singleton instance.
    /// </summary>
    /// <returns>The singleton.</returns>
    public static WebAssemblyRuntime GetInstance()
    {
        if (instance == null)
        {
            instance = new WebAssemblyRuntime();
        }

        return instance;
    }

    /// <summary>
    /// Invokes the specified JavaScript function synchronously.
    /// </summary>
    /// <param name="identifier">JavaScript function.</param>
    /// <param name="warn">Show warning if function is not defined.</param>
    /// <param name="args">Function arguments.</param>
    public void Invoke(string identifier, bool warn = true, params object[] args)
    {
        try
        {
            this.Runtime.InvokeVoid(identifier, args);
        }
        catch (Exception e)
        {
            if (warn)
            {
                Console.WriteLine($"WARNING: function {identifier}({string.Join(",", args)}): {e.Message}");
            }
        }
    }

    /// <summary>
    /// Invokes the specified JavaScript function synchronously.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    /// <param name="identifier">JavaScript function.</param>
    /// <param name="warn">Show warning if function is not defined.</param>
    /// <param name="args">Function arguments.</param>
    /// <returns>Function result.</returns>
    public T? Invoke<T>(string identifier, bool warn = true, params object[] args)
    {
        try
        {
            if (typeof(T) == typeof(JSObject))
            {
                return (T)Convert.ChangeType(new JSObject(this.Runtime.Invoke<IJSInProcessObjectReference>(identifier, args)), typeof(T));
            }

            return this.Runtime.Invoke<T>(identifier, args);
        }
        catch (Exception e)
        {
            if (warn)
            {
                Console.WriteLine($"WARNING: function {identifier}({string.Join(",", args)}): {e.Message}");
            }

            return default(T);
        }
    }

    /// <summary>
    /// Get global window object.
    /// </summary>
    /// <param name="warn">Show warning if property is not defined.</param>
    /// <returns>window object.</returns>
    public JSObject? GetGlobalObject(bool warn = true)
    {
        return this.Invoke<JSObject>("window._getGlobalObject", warn: warn);
    }

    /// <summary>
    /// Gets an JSObject by its id.
    /// </summary>
    /// <param name="id">Id of the object.</param>
    /// <param name="warn">Show warning if property is not defined.</param>
    /// <returns>JSObject from the DOM.</returns>
    public JSObject? GetElementById(string id, bool warn = true)
    {
        return this.Invoke<JSObject>("document.getElementById", warn: warn, id);
    }

    /// <summary>
    /// Configures the callback for draw loop and starts it. If callback is null the loop is stopped.
    /// </summary>
    /// <param name="callback">Draw loop callback.</param>
    public void SetRequestAnimationFrameCallback(Action<double> callback)
    {
        if (this.dotnetRefrence == null)
        {
            this.dotnetRefrence = DotNetObjectReference.Create<WebAssemblyRuntime>(this);
        }

        this.Invoke("window._setRequestAnimationFrameCallback", warn: true, this.dotnetRefrence, callback != null ? nameof(WebAssemblyRuntime.InvokeRequestAnimationFrameCallback) : string.Empty);

        this.requestAnimationFrameAction = callback;
    }

    /// <summary>
    /// Invoke draw call.
    /// </summary>
    /// <param name="d">Timestamp.</param>
    [JSInvokable]
    public void InvokeRequestAnimationFrameCallback(double d)
    {
        this.requestAnimationFrameAction?.Invoke(d);
    }

    /// <summary>
    /// Dispose object.
    /// </summary>
    public void Dispose()
    {
        this.dotnetRefrence?.Dispose();

        // TODO delete singleton instance?
    }

    private WebAssemblyRuntime()
    {
        var args = new string[] { "Web", "WebGL2" };
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        var host = builder.Build();
        host.RunAsync();

        this.Runtime = (WebAssemblyJSRuntime)host.Services.GetRequiredService<IJSRuntime>();
    }
}
