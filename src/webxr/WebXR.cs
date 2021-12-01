using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


[AttributeUsage(AttributeTargets.Method)]
sealed class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type t) { }
}

public delegate void xrFrameCallback(double ms);

public class WebXR
{
    private const string DllName = "libWebXR";

    private const int position_length = 3;
    private const int orientation_length = 4;
    private const int projection_length = 16;

    private IntPtr position_buffer;
    private IntPtr orientation_buffer;
    private IntPtr projection_buffer;

    private unsafe double* position;
    private unsafe double* orientation;
    private unsafe double* projectionMatrix;

    private xrFrameCallback OnFrame;

    [DllImport(DllName)]
    private unsafe static extern int xrInit(double* position_buffer, double* orientation_buffer, double* projection_buffer);

    [DllImport(DllName)]
    private static extern void xrRequestImmersiveSessionAsync(string mode, string canvasId, string contextType);

    [DllImport(DllName)]
    private static extern int xrIsSessionReady();

    [DllImport(DllName)]
    private static extern void xrRequestAnimationFrame(xrFrameCallback onFrame);

    [DllImport(DllName)]
    private static extern int xrIsPoseAvailable();

    [DllImport(DllName)]
    private static extern void xrEndImmersiveSession();

    // Singleton
    private static WebXR _instance;
    public static WebXR GetInstance()
    {
        if (_instance == null)
        {
            _instance = new WebXR();
        }

        return _instance;
    }

    public unsafe void Init(xrFrameCallback onFrame)
    {
        if (xrInit(position, orientation, projectionMatrix) == 0)
        {
            throw new InvalidOperationException("WebXR unavailable on this User Agent");
        }

        this.OnFrame = onFrame;
    }

    public async Task<bool> RequestImmersiveSessionAsync(string immersiveMode, string canvasId, string contextType)
    {
        xrRequestImmersiveSessionAsync(immersiveMode, canvasId, contextType);
        while (xrIsSessionReady() == 0)
        {
            //TODO call isSessionSupported on C side
            await Task.Delay(100);
        }

        return true;
    }

    public void StartRequestAnimationFrame() =>
            xrRequestAnimationFrame(this.OnFrame);

    public void StopRequestAnimationFrame() =>
        xrRequestAnimationFrame(null);

    public unsafe bool SetViewerPose(ref ViewProperties p)
    {
        bool isPoseAvailable = xrIsPoseAvailable() == 1;

        if (isPoseAvailable)
        {
            p.Pose.Position.X = Convert.ToSingle(this.position[0]);
            p.Pose.Position.Y = Convert.ToSingle(this.position[1]);
            p.Pose.Position.Z = Convert.ToSingle(this.position[2]);

            p.Pose.Orientation.X = Convert.ToSingle(this.orientation[0]);
            p.Pose.Orientation.Y = Convert.ToSingle(this.orientation[1]);
            p.Pose.Orientation.Z = Convert.ToSingle(this.orientation[2]);
            p.Pose.Orientation.W = Convert.ToSingle(this.orientation[3]);

            p.Projection = new Matrix4x4(
                        Convert.ToSingle(this.projectionMatrix[0]),
                        Convert.ToSingle(this.projectionMatrix[1]),
                        Convert.ToSingle(this.projectionMatrix[2]),
                        Convert.ToSingle(this.projectionMatrix[3]),
                        Convert.ToSingle(this.projectionMatrix[4]),
                        Convert.ToSingle(this.projectionMatrix[5]),
                        Convert.ToSingle(this.projectionMatrix[6]),
                        Convert.ToSingle(this.projectionMatrix[7]),
                        Convert.ToSingle(this.projectionMatrix[8]),
                        Convert.ToSingle(this.projectionMatrix[9]),
                        Convert.ToSingle(this.projectionMatrix[10]),
                        Convert.ToSingle(this.projectionMatrix[11]),
                        Convert.ToSingle(this.projectionMatrix[12]),
                        Convert.ToSingle(this.projectionMatrix[13]),
                        Convert.ToSingle(this.projectionMatrix[14]),
                        Convert.ToSingle(this.projectionMatrix[15]));
        }

        return isPoseAvailable;
    }

    public void EndImmersiveSession()
    {
        xrEndImmersiveSession();
    }

    private unsafe WebXR()
    {
        this.position_buffer = Marshal.AllocHGlobal(position_length * sizeof(double));
        this.orientation_buffer = Marshal.AllocHGlobal(orientation_length * sizeof(double));
        this.projection_buffer = Marshal.AllocHGlobal(projection_length * sizeof(double));

        this.position = (double*)this.position_buffer.ToPointer();
        this.orientation = (double*)this.orientation_buffer.ToPointer();
        this.projectionMatrix = (double*)this.projection_buffer.ToPointer();
    }

    public unsafe void Dispose()
    {
        this.position = null;
        this.orientation = null;
        this.projectionMatrix = null;

        Marshal.FreeHGlobal(this.position_buffer);
        Marshal.FreeHGlobal(this.orientation_buffer);
        Marshal.FreeHGlobal(this.projection_buffer);
    }
}
