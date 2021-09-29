using Microsoft.JSInterop;

/// <summary>
/// Represents a reference to a JavaScript object.
/// </summary>
public class JSObject : IDisposable
{
    private Dictionary<string, Action<JSObject>> activeListeners = new Dictionary<string, Action<JSObject>>();
    private Dictionary<string, Action> activeSimpleListeners = new Dictionary<string, Action>();
    private DotNetObjectReference<JSObject>? dotnetRefrence = null;

    /// <summary>
    /// Gets the JavaScrtipt reference of the object.
    /// </summary>
    public IJSInProcessObjectReference Reference { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JSObject"/> class.
    /// </summary>
    /// <param name="reference">Internal object reference.</param>
    public JSObject(IJSInProcessObjectReference reference)
    {
        this.Reference = reference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JSObject"/> class.
    /// </summary>
    /// <param name="reference">Internal object reference.</param>
    public JSObject(IJSObjectReference reference)
    {
        this.Reference = (IJSInProcessObjectReference)reference;
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
            this.Reference.InvokeVoid(identifier, args);
        }
        catch (Exception e)
        {
            if (warn)
            {
                Console.WriteLine($"WARNING: function {identifier}({string.Join(", ", args)}): {e.Message}");
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
    /// <returns>Function result. default(T) if it does not exists.</returns>
    public T? Invoke<T>(string identifier, bool warn = true, params object[] args)
    {
        try
        {
            if (typeof(T) == typeof(JSObject))
            {
                return (T)Convert.ChangeType(new JSObject(this.Reference.Invoke<IJSInProcessObjectReference>(identifier, args)), typeof(T));
            }

            return this.Reference.Invoke<T>(identifier, args);
        }
        catch (Exception e)
        {
            if (warn)
            {
                Console.WriteLine($"WARNING:function {identifier}({string.Join(",", args)}): {e.Message}");
            }

            return default;
        }
    }

    /// <summary>
    /// Access JSObject property.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="property">Name of the property.</param>
    /// <param name="warn">Show warning if property is not defined.</param>
    /// <returns>Property value. Null if it does not exists.</returns>
    public T? GetObjectProperty<T>(string property, bool warn = true)
    {
        var wasm = WebAssemblyRuntime.GetInstance();

        return wasm.Invoke<T>("window._getObjectProperty", warn: warn, this.Reference, property);
    }

    /// <summary>
    /// Sets JSObject property.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="property">Name of the property.</param>
    /// <param name="value">Value of the property.</param>
    /// <param name="warn">Show warning if function is not defined.</param>
    public void SetObjectProperty<T>(string property, T value, bool warn = true)
    {
        var wasm = WebAssemblyRuntime.GetInstance();

        wasm.Invoke<T>("window._setObjectProperty", warn: warn, this.Reference, property, value);
    }

    /// <summary>
    /// Add simple event listener to object.
    /// The listener does not use event information but it the most efficient implementation.
    /// </summary>
    /// <param name="eventName">Name of the event that triggers the listener.</param>
    /// <param name="listener">Action method to be called.</param>
    /// <param name="options">Optional parameters.</param>
    public void AddSimpleEventListener(string eventName, Action listener, params object[] options)
    {
        var wasm = WebAssemblyRuntime.GetInstance();

        if (this.dotnetRefrence == null)
        {
            this.dotnetRefrence = DotNetObjectReference.Create<JSObject>(this);
        }

        wasm.Invoke("window._addSimpleEventListener", warn: true, this.Reference, eventName, this.dotnetRefrence, nameof(JSObject.InvokeEventListenerSimple), options);

        this.activeSimpleListeners.Add(eventName, listener);
    }

    /// <summary>
    /// Add event listener to object.
    /// This version is the slowest and should be avoided when event is going to trigger a lot.
    /// </summary>
    /// <param name="eventName">Name of the event that triggers the listener.</param>
    /// <param name="listener">Action method to be called.</param>
    /// <param name="options">Optional parameters.</param>
    public void AddEventListener(string eventName, Action<JSObject> listener, params object[] options)
    {
        var wasm = WebAssemblyRuntime.GetInstance();

        if (this.dotnetRefrence == null)
        {
            this.dotnetRefrence = DotNetObjectReference.Create<JSObject>(this);
        }

        wasm.Invoke("window._addEventListener", warn: true, this.Reference, eventName, this.dotnetRefrence, nameof(JSObject.InvokeEventListener), options);

        this.activeListeners.Add(eventName, listener);
    }

    /// <summary>
    /// Remove event listener from object.
    /// </summary>
    /// <param name="eventName">Name of the event that triggers the listener.</param>
    /// <param name="options">Optional parameters.</param>
    public void RemoveEventListener(string eventName, params object[] options)
    {
        var wasm = WebAssemblyRuntime.GetInstance();

        if (this.dotnetRefrence == null)
        {
            this.dotnetRefrence = DotNetObjectReference.Create<JSObject>(this);
        }

        wasm.Invoke("window._removeEventListener", warn: true, this.Reference, eventName, options);

        if (this.activeSimpleListeners.ContainsKey(eventName))
        {
            this.activeSimpleListeners.Remove(eventName);
        }
        else
        {
            this.activeListeners.Remove(eventName);
        }
    }

    /// <summary>
    /// Faster implementation of callback for event listeners, but without event information.
    /// </summary>
    /// <param name="eventName">Type of event.</param>
    [JSInvokable]
    public void InvokeEventListenerSimple(string eventName)
    {
        this.activeSimpleListeners[eventName].Invoke();
    }

    /// <summary>
    /// Callback for event listeners.
    /// </summary>
    /// <param name="eventName">Type of event.</param>
    /// <param name="el">Event element.</param>
    [JSInvokable]
    public void InvokeEventListener(string eventName, IJSObjectReference el)
    {
        this.activeListeners[eventName].Invoke(new JSObject(el));
    }

    /// <summary>
    /// Dispose the object.
    /// </summary>
    public void Dispose()
    {
        this.dotnetRefrence?.Dispose();
    }
}
