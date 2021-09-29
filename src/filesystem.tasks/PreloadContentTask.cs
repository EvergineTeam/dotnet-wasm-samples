//using System.Text.Json;
//using System.Text.Json.Serialization;
using System.IO;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using Microsoft.Build.Utilities;

public class PreloadContentTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] Assets { get; set; }

    [Required]
    public string OutDir { get; set; }

    [Required]
    public string MainJS { get; set; }

    [Output]
    public ITaskItem GeneratedContentFile { get; set; }

    public override bool Execute()
    {
        if (Assets.Length == 0)
        {
            Log.LogError("Assets is empty.");
            return false;
        }

        //var assets = [{ url: "assets/file.txt", path: "assets", name: "file.txt" }];
        List<string> assetsList = new List<string>(Assets.Length);
        foreach (var asset in Assets)
        {
            var fileName = asset.GetMetadata("Filename");
            var extension = asset.GetMetadata("Extension");
            var url = asset.GetMetadata("RelativePath").Replace('\\', '/');
            var path = asset.GetMetadata("FullPath");

            var name = $"{fileName}{extension}";
            var parentDir = url.Substring(0, url.Length - name.Length);
            var size = new FileInfo(path).Length;

            assetsList.Add($"{{url:'{url}',path:'{parentDir}',name:'{name}',size:{size}}}");
        }

        var assetsJS = $"var assets = [{string.Join(",", assetsList)}];\n{File.ReadAllText(MainJS)}";

        var outPath = Path.Combine(OutDir, "assets.js");
        File.WriteAllText(outPath, assetsJS);
        this.GeneratedContentFile = new TaskItem(outPath);

        return true;
    }
}
