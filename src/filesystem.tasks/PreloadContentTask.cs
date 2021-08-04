using System.Text.Json;
using System.Text.Json.Serialization;
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

        List<string> assetsList = new List<string>(Assets.Length);
        foreach (var asset in Assets)
        {
            var path = asset.GetMetadata("RelativePath");
            assetsList.Add(path);
            Log.LogWarning(path);
        }

        var jsonString = JsonSerializer.Serialize(assetsList);
        File.WriteAllText(Path.Combine(OutDir, "assets.json"), jsonString);

        this.GeneratedContentFile = new TaskItem(Path.Combine(OutDir, "assets.json"));

        return true;
    }
}
