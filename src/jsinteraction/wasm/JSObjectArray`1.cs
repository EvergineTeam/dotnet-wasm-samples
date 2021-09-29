/// <summary>
/// Represents an array of JSObject.
/// </summary>
/// <typeparam name="T">Array type.</typeparam>
public class JSObjectArray<T> : IDisposable
{
    private JSObject jsobject;
    private int? length = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="JSObjectArray{T}"/> class.
    /// </summary>
    /// <param name="obj">JSObject that is an array of T.</param>
    public JSObjectArray(JSObject obj)
    {
        this.jsobject = obj;
    }

    /// <summary>
    /// Gets the length of the Array.
    /// </summary>
    public int Length
    {
        get
        {
            if (!this.length.HasValue)
            {
                this.length = this.jsobject.GetObjectProperty<int>("length");
            }

            return this.length.Value;
        }
    }

    /// <summary>
    /// Indexer.
    /// </summary>
    /// <param name="i">index.</param>
    /// <returns>Array object.</returns>
    public T? this[int i]
    {
        get { return this.jsobject.GetObjectProperty<T>(i.ToString()); }
        set { this.jsobject.SetObjectProperty(i.ToString(), value); }
    }

    /// <summary>
    /// Dispose object.
    /// </summary>
    public void Dispose()
    {
        this.jsobject.Dispose();
    }
}
