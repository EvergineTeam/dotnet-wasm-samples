// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;

public static class Program
{
    private static bool ready;
    private static IWebXR webxr;
    private static bool isRunning;
    private static ViewProperties viewProperties;

    public static async Task<int> Main(string[] args)
    {
        webxr = WebXR.GetInstance();
        await Task.Delay(1);
        Console.WriteLine("Hello World!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        // // Console.WriteLine($"The answer to everything is {NativeBinding.hello()}");

        // // NativeBinding.callCbk(cbk);
        // // //NativeBinding.callCbk(&cbk);

        return args.Length;
    }

    public static Task EnterImmersive(string immersiveMode, string canvasId)
    {
        try
        {
            webxr.Init(OnFrame);
            ready = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.CompletedTask;
        }

        var contextType = "webgl2";

        return webxr.RequestImmersiveSessionAsync(immersiveMode, canvasId, contextType)
            .ContinueWith(antedecent =>
            {
                if (antedecent.Result)
                {
                    isRunning = true;
                    webxr.RequestAnimationFrame();
                }
            });
    }

    public static void ExitImmersive()
    {
        if (!ready)
        {
            return;
        }

        isRunning = false;
        webxr.EndImmersiveSession();
    }

    [MonoPInvokeCallback(typeof(xrFrameCallback))]
    public static void OnFrame(double ellapsedMilliseconds)
    {
        try
        {
            if (isRunning)
            {
                bool isPoseAvailable = webxr.SetViewerPose(ref viewProperties);

                if (isPoseAvailable)
                {
                    Console.WriteLine("Render callback!");
                }
                webxr.RequestAnimationFrame();
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                "Workaround exception to leverage the actual one, not the wrong Delegate-related one",
                exception);
        }
    }
}