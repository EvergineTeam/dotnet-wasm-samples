/* Stuff considered while porting this file from Python:
 * - Everything is kept into this *single* file, single class, single Main method (among auxiliary functions): to keep
 *   its original nature :-)
 * - Var names are left equal, except been C-Sharpified: this way will help us keep changes from the original source
 * - Comments are stripped: to avoid their maintenance
 *
 * Ahead of its origin:
 * - Module['setProgress'] is called, if available, when reporting load status:
 *
 *   function (loadedBytes, totalBytes)
 *
 *   It enables adding progress bars to visually enhance the loading wait for big preload files.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

    public class FilePackagerTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string DataTargetPath {get; set;}

        public string PreloadPath {get; set;} = null;
        public string JsOutput {get; set;} = null;

        public bool NoHeapCopy {get; set;} = false;
        public bool SeparateMetadata {get; set;} = false;

        const string ExportName = "Module";
        const string Separator = "/";

        readonly string[] AudioSuffixes = new string[] { ".ogg", ".wav", ".mp3" };

        string dataTarget, preload, jsOutput;
        bool heapCopy = true;
        List<(string srcPath, string dstPath, string mode, bool explicitDstPath, int dataStart, int dataEnd)> newDataFiles =
            new List<(string srcPath, string dstPath, string mode, bool explicitDstPath, int dataStart, int dataEnd)>();
        bool hasPreloaded = false;
        bool lz4 = false;
        bool force = true;
        bool separateMetaData = false;

        public override bool Execute()
        {
            var shouldShowHelp = false;

            this.dataTarget = this.DataTargetPath;

            if (this.PreloadPath != null) {
                this.preload = this.PreloadPath;
                this.hasPreloaded = true;
            }

            if (this.JsOutput != null) {
                this.jsOutput = this.JsOutput;
            }

            this.heapCopy = !this.NoHeapCopy;
            this.separateMetaData = this.SeparateMetadata;

            if (string.IsNullOrWhiteSpace(dataTarget))
            {
                Console.WriteLine("ERROR: Target cannot be null.");
                return false;
            }

            var mode = "preload";
            var preloadFile = new string(preload);
            var atPosition = preloadFile.Replace("@@", "__").IndexOf('@');
            var usesAtNotation = atPosition != -1;
            string srcPath, dstPath;
            var dataFiles = new List<(string srcPath, string dstPath, string mode, bool explicitDstPath,
                int dataStart, int dataEnd)>();

            if (usesAtNotation)
            {
                srcPath = preloadFile.Substring(0, atPosition).Replace("@@", "@");
                dstPath = preloadFile.Substring(atPosition + 1).Replace("@@", "@");
            }
            else
            {
                srcPath = dstPath = preloadFile.Replace("@@", "@");
            }

            if (File.Exists(srcPath) || Directory.Exists(srcPath))
            {
                dataFiles.Add((srcPath, dstPath, mode, usesAtNotation, -1, -1));
            }
            else
            {
                Console.WriteLine($"Warning: {preloadFile} does not exist, ignoring.");
            }

            var ret = @$"
var Module = typeof {ExportName} !== 'undefined' ? {ExportName} : {{}};
";
            ret += @"
if (!Module.expectedDataFileDownloads) {
  Module.expectedDataFileDownloads = 0;
  Module.finishedDataFileDownloads = 0;
}
Module.expectedDataFileDownloads++;
(function() {
 var loadPackage = function(metadata) {
";
            var code = @"
    function assert(check, msg) {
      if (!check) throw msg + new Error().stack;
    }
";
            foreach (var file in dataFiles)
            {
                if (!ShouldIgnore(file.srcPath))
                {
                    if (Directory.Exists(file.srcPath))
                    {
                        Add(file.mode, file.srcPath, file.dstPath);
                    }
                    else
                    {
                        newDataFiles.Add(file);
                    }
                }
            }

            dataFiles = newDataFiles.Where(file => !Directory.Exists(file.srcPath)).ToList();

            if (dataFiles.Count == 0)
            {
                Console.WriteLine("Nothing to do!");
                Environment.Exit(1);
            }

            var currAbsPath = Directory.GetCurrentDirectory();

            for (var i = 0; i < dataFiles.Count; i++)
            {
                var file = dataFiles[i];

                if (!file.explicitDstPath)
                {
                    var path = file.dstPath;
                    var absPath = Path.GetFullPath(path);
                    Debug.WriteLine(path, absPath, currAbsPath);

                    if (!absPath.StartsWith(currAbsPath))
                    {
                        Console.WriteLine($"Error: Embedding \"{path}\" which is below the current directory " +
                            $"\"{currAbsPath}\". This is invalid since the current directory becomes the " +
                            "root that the generated code will see");
                        Environment.Exit(1);
                    }

                    file.dstPath = absPath.Substring(currAbsPath.Length + 1);

                    if (Path.IsPathRooted(path))
                    {
                        Console.WriteLine($"Warning: Embedding an absolute file/directory name \"{path}\" to the " +
                            "virtual filesystem. The file will be made available in the " +
                            $"relative path \"{file.dstPath}\". You can use the explicit syntax " +
                            "--preload-file srcpath@dstpath to explicitly specify the target " +
                            "location the absolute source path should be directed to.");
                    }
                }
            }

            for (var i = 0; i < dataFiles.Count; i++)
            {
                var file = dataFiles[i];
                file.dstPath = file.dstPath.Replace(Path.DirectorySeparatorChar.ToString(), Separator);

                if (file.dstPath.EndsWith(Separator))
                {
                    file.dstPath = file.dstPath + Path.GetFileName(file.srcPath);
                }

                if (!file.dstPath.StartsWith(Separator))
                {
                    file.dstPath = Separator + file.dstPath;
                }

                dataFiles[i] = file;
                Debug.WriteLine($"Packaging file \"{file.srcPath}\" to VFS in path \"{file.dstPath}\".");
            }

            // Remove duplicates
            dataFiles = dataFiles
                .GroupBy(file => file.dstPath)
                .Select(group => group.First())
                .ToList();

            var metadata = new Dictionary<string, object>()
            {
                { "files", new List<object>() }
            };

            var partialDirs = new List<string>();

            foreach (var file in dataFiles)
            {
                var dirName = GetDirectoryNameWithForwardSlashes(file.dstPath);
                dirName = dirName.TrimStart(Separator[0]);

                if (dirName != string.Empty)
                {
                    var parts = dirName.Split(Separator);

                    for (var i = 0; i < parts.Length; i++)
                    {
                        var partial = string.Join(Separator, parts.Take(i + 1));

                        if (!partialDirs.Contains(partial))
                        {
                            code += $"Module['FS_createPath'](" +
                                $"'{Separator + string.Join(Separator, parts.Take(i))}', '{parts[i]}', true, true);\n";
                            partialDirs.Add(partial);
                        }
                    }
                }
            }

            if (hasPreloaded)
            {
                var data = File.OpenWrite(dataTarget);
                var start = 0;

                for (var i = 0; i < dataFiles.Count; i++)
                {
                    var file = dataFiles[i];
                    file.dataStart = start;
                    var curr = File.ReadAllBytes(file.srcPath);
                    file.dataEnd = start + curr.Length;
                    dataFiles[i] = file;
                    start += curr.Length;
                    data.Write(curr);
                }

                data.Close();

                if (start > 256 * 1024 * 1024)
                {
                    Console.WriteLine(
                        $"warning: file packager is creating an asset bundle of {start / (1024 * 1024)} MB. " +
                        "this is very large, and browsers might have trouble loading it. See " +
                        "https://hacks.mozilla.org/2015/02/synchronous-execution-and-filesystem-access-in-emscripten/");
                }

                var createData = @"
        Module['FS_createDataFile'](this.name, null, byteArray, true, true, true); // canOwn this data in the filesystem, it is a slide into the heap that will never change
        Module['removeRunDependency']('fp ' + that.name);
";

                code += @$"
    function DataRequest(start, end, audio) {{
      this.start = start;
      this.end = end;
      this.audio = audio;
    }}
    DataRequest.prototype = {{
      requests: {{}},
      open: function(mode, name) {{
        this.name = name;
        this.requests[name] = this;
        Module['addRunDependency']('fp ' + this.name);
      }},
      send: function() {{}},
      onload: function() {{
        var byteArray = this.byteArray.subarray(this.start, this.end);
        this.finish(byteArray);
      }},
      finish: function(byteArray) {{
        var that = this;
{createData}
        this.requests[this.name] = null;
      }}
    }};

        var files = metadata.files;
        for (var i = 0; i < files.length; ++i) {{
          new DataRequest(files[i].start, files[i].end, files[i].audio).open('GET', files[i].filename);
        }}
";
            }

            var counter = 0;

            foreach (var file in dataFiles)
            {
                var fileName = file.dstPath;
                var dirName = GetDirectoryNameWithForwardSlashes(fileName);
                var baseName = Path.GetFileName(fileName);

                if (file.mode == "preload")
                {
                    var varName = $"filePreload{counter}";
                    counter += 1;
                    ((List<object>)metadata["files"]).Add(new
                    {
                        filename = file.dstPath,
                        start = file.dataStart,
                        end = file.dataEnd,
                        audio = AudioSuffixes.Contains(fileName.Substring(fileName.Length - 4)) ? 1 : 0,
                    });
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            string useData = null;

            if (hasPreloaded)
            {
                if (!lz4)
                {
                    if (heapCopy)
                    {
                        useData = @"
        // copy the entire loaded file into a spot in the heap. Files will refer to slices in that. They cannot be freed though
        // (we may be allocating before malloc is ready, during startup).
        var ptr = Module['getMemory'](byteArray.length);
        Module['HEAPU8'].set(byteArray, ptr);
        DataRequest.prototype.byteArray = Module['HEAPU8'].subarray(ptr, ptr+byteArray.length);
  ";
                    }
                    else
                    {
                        useData = @"
        // Reuse the bytearray from the XHR as the source for file reads.
        DataRequest.prototype.byteArray = byteArray;
  ";
                        useData += @"
          var files = metadata.files;
          for (var i = 0; i < files.length; ++i) {
            DataRequest.prototype.requests[files[i].filename].onload();
          }
    ";
                        useData += "          Module['removeRunDependency']" +
                            $"('datafile_{EscapeForJSString(dataTarget)}');\n";
                    }
                }

                var packageUuid = Guid.NewGuid();
                var packageName = dataTarget;
                var remotePackageSize = new FileInfo(packageName).Length;
                var remotePackageName = Path.GetFileName(packageName);
                ret += @$"
    var PACKAGE_PATH;
    if (typeof window === 'object') {{
      PACKAGE_PATH = window['encodeURIComponent'](window.location.pathname.toString().substring(0, window.location.pathname.toString().lastIndexOf('/')) + '/');
    }} else if (typeof location !== 'undefined') {{
      // worker
      PACKAGE_PATH = encodeURIComponent(location.pathname.toString().substring(0, location.pathname.toString().lastIndexOf('/')) + '/');
    }} else {{
      throw 'using preloaded data can only be done on a web page or in a web worker';
    }}
    var PACKAGE_NAME = '{EscapeForJSString(dataTarget)}';
    var REMOTE_PACKAGE_BASE = '{EscapeForJSString(remotePackageName)}';
    if (typeof Module['locateFilePackage'] === 'function' && !Module['locateFile']) {{
      Module['locateFile'] = Module['locateFilePackage'];
      err('warning: you defined Module.locateFilePackage, that has been renamed to Module.locateFile (using your locateFilePackage for now)');
    }}
    var REMOTE_PACKAGE_NAME = Module['locateFile'] ? Module['locateFile'](REMOTE_PACKAGE_BASE, '') : REMOTE_PACKAGE_BASE;
  ";
                metadata.Add("remote_package_size", remotePackageSize);
                metadata.Add("package_uuid", packageUuid);
                ret += @"
    var REMOTE_PACKAGE_SIZE = metadata.remote_package_size;
    var PACKAGE_UUID = metadata.package_uuid;
  ";

                ret += @"
    function fetchRemotePackage(packageName, packageSize, callback, errback) {
      var xhr = new XMLHttpRequest();
      xhr.open('GET', packageName, true);
      xhr.responseType = 'arraybuffer';
      xhr.onprogress = function(event) {
        var url = packageName;
        var size = packageSize;
        if (event.total) size = event.total;
        if (event.loaded) {
          if (!xhr.addedTotal) {
            xhr.addedTotal = true;
            if (!Module.dataFileDownloads) Module.dataFileDownloads = {};
            Module.dataFileDownloads[url] = {
              loaded: event.loaded,
              total: size
            };
          } else {
            Module.dataFileDownloads[url].loaded = event.loaded;
          }
          var total = 0;
          var loaded = 0;
          var num = 0;
          for (var download in Module.dataFileDownloads) {
          var data = Module.dataFileDownloads[download];
            total += data.total;
            loaded += data.loaded;
            num++;
          }
          total = Math.ceil(total * Module.expectedDataFileDownloads/num);
          if (Module['setStatus']) Module['setStatus']('Downloading data... (' + loaded + '/' + total + ')');
          if (Module['setProgress']) Module['setProgress'](loaded, total);
        } else if (!Module.dataFileDownloads) {
          if (Module['setStatus']) Module['setStatus']('Downloading data...');
        }
      };
      xhr.onerror = function(event) {
        throw new Error(""NetworkError for: "" + packageName);
      }
      xhr.onload = function(event) {
        if (xhr.status == 200 || xhr.status == 304 || xhr.status == 206 || (xhr.status == 0 && xhr.response)) { // file URLs can return 0
          var packageData = xhr.response;
          callback(packageData);
        } else {
          throw new Error(xhr.statusText + "" : "" + xhr.responseURL);
        }
      };
      xhr.send(null);
    };

    function handleError(error) {
      console.error('package error:', error);
    };
";

                code += @$"
    function processPackageData(arrayBuffer) {{
      Module.finishedDataFileDownloads++;
      assert(arrayBuffer, 'Loading data file failed.');
      assert(arrayBuffer instanceof ArrayBuffer, 'bad input to processPackageData');
      var byteArray = new Uint8Array(arrayBuffer);
      var curr;
      {useData}
    }};
    Module['addRunDependency']('datafile_{EscapeForJSString(dataTarget)}');
  ";
                code += @"
    if (!Module.preloadResults) Module.preloadResults = {};
  ";

                ret += @"
      var fetchedCallback = null;
      var fetched = Module['getPreloadedPackage'] ? Module['getPreloadedPackage'](REMOTE_PACKAGE_NAME, REMOTE_PACKAGE_SIZE) : null;

      if (!fetched) fetchRemotePackage(REMOTE_PACKAGE_NAME, REMOTE_PACKAGE_SIZE, function(data) {
        if (fetchedCallback) {
          fetchedCallback(data);
          fetchedCallback = null;
        } else {
          fetched = data;
        }
      }, handleError);
    ";

                code += @"
      Module.preloadResults[PACKAGE_NAME] = {fromCache: false};
      if (fetched) {
        processPackageData(fetched);
        fetched = null;
      } else {
        fetchedCallback = processPackageData;
      }
    ";
            }

            ret += @"
  function runWithFS() {
";
            ret += code;
            ret += @"
  }
  if (Module['calledRun']) {
    runWithFS();
  } else {
    if (!Module['preRun']) Module['preRun'] = [];
    Module[""preRun""].push(runWithFS); // FS is not initialized yet, wait for it
  }
";
            var metadataTemplate = string.Empty;
            if (separateMetaData) {
                var metadata_file = Path.GetFileName(jsOutput) + ".metadata";
                metadataTemplate = $@"
  Module['removeRunDependency']('{metadata_file}');
 }}

 function runMetaWithFS() {{
  Module['addRunDependency']('{metadata_file}');
  var REMOTE_METADATA_NAME = Module['locateFile'] ? Module['locateFile']('{metadata_file}', '') : '{metadata_file}';
  var xhr = new XMLHttpRequest();
  xhr.onreadystatechange = function() {{
   if (xhr.readyState === 4 && xhr.status === 200) {{
     loadPackage(JSON.parse(xhr.responseText));
   }}
  }}
  xhr.open('GET', REMOTE_METADATA_NAME, true);
  xhr.overrideMimeType('application/json');
  xhr.send(null);
 }}

 if (Module['calledRun']) {{
  runMetaWithFS();
 }} else {{
  if (!Module['preRun']) Module['preRun'] = [];
  Module[""preRun""].push(runMetaWithFS);
 }}
";
            } else {

                metadataTemplate = $@"
 }}
 loadPackage({JsonSerializer.Serialize(metadata)});
";
            }
            ret += $@"{metadataTemplate}
}})();
";

            if (force || dataFiles.Any())
            {
                if (string.IsNullOrEmpty(jsOutput))
                {
                    Console.WriteLine(ret);
                }
                else
                {
                    if (File.Exists(jsOutput))
                    {
                        var old = File.ReadAllText(jsOutput);

                        if (old != ret)
                        {
                            File.WriteAllText(jsOutput, ret);
                        }
                    }
                    else
                    {
                        File.WriteAllText(jsOutput, ret);
                    }
                    if (separateMetaData)
                    {
                        File.WriteAllText(jsOutput + ".metadata", JsonSerializer.Serialize(metadata));
                    }
                }
            }

            return true;
        }

        string GetDirectoryNameWithForwardSlashes(string path) =>
            Path.GetDirectoryName(path).Replace("\\", Separator);

        string EscapeForJSString(string s) =>
            s.Replace("\\", Separator).Replace("'", "\\'").Replace("\"", "\\\"");

        void Add(string mode, string rootPathSrc, string rootPathDst)
        {
            var dirNames = new List<string>();
            dirNames.Add(rootPathSrc);

            foreach (var fullName in Directory.EnumerateDirectories(rootPathSrc, "*", SearchOption.AllDirectories))
            {
                if (!ShouldIgnore(fullName))
                {
                    dirNames.Add(fullName);
                }
                else
                {
                    Debug.WriteLine(
                        $"Skipping directory \"{fullName}\" from inclusion in the emscripten virtual file system.");
                }
            }

            foreach (var dirName in dirNames)
            {
                foreach (var fullName in Directory.EnumerateFiles(dirName))
                {
                    if (!ShouldIgnore(fullName))
                    {
                        var dstPath = string.Join(
                            Separator,
                            rootPathDst,
                            Path.GetRelativePath(rootPathSrc, fullName).Replace("\\", Separator));
                        newDataFiles.Add((fullName, dstPath, mode, true, -1, -1));
                    }
                    else
                    {
                        Debug.WriteLine($"Skipping file \"{fullName}\" from inclusion in the emscripten " +
                            "virtual file system.");
                    }
                }
            }
        }

        bool ShouldIgnore(string fullName)
        {
            var attributes = File.GetAttributes(fullName);

            if (attributes.HasFlag(FileAttributes.Hidden))
            {
                return true;
            }

            return false;
        }
    }
