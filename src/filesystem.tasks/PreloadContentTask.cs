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

    // public string PreloadPath {get; set;} = null;
    // public string JsOutput {get; set;} = null;

    // public bool NoHeapCopy {get; set;} = false;
    // public bool SeparateMetadata {get; set;} = false;

    public override bool Execute()
    {
        Log.LogWarning(OutDir);

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

            var name = $"{fileName}{extension}";
            var parentDir = url.Substring(0, url.Length - name.Length);

            assetsList.Add($"{{url:'{url}',path:'{parentDir}',name:'{name}'}}");
            Log.LogWarning(url);
        }

        var assetsJS = $"var assets = [{string.Join(",", assetsList)}];\n{File.ReadAllText(MainJS)}";

        //var jsonString = JsonSerializer.Serialize(assetsList);

        var outPath = Path.Combine(OutDir, "assets.js");
        File.WriteAllText(outPath, assetsJS);
        this.GeneratedContentFile = new TaskItem(outPath);

        return true;
    }
}
