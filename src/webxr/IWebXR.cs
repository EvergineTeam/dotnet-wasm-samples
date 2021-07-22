using System;
using System.Threading.Tasks;

public delegate void xrFrameCallback(double ms);

public interface IWebXR : IDisposable
{
    void Init(xrFrameCallback onFrame);
    Task<bool> RequestImmersiveSessionAsync(string immersiveMode, string canvasId, string contextType);
    void RequestAnimationFrame();
    bool SetViewerPose(ref ViewProperties p);
    void EndImmersiveSession();
}
