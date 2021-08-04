var timeout_ms = 5 * 1000;
// // var assets = [{ url: "assets/file.txt", path: "assets", name: "file.txt" }];

// This is the promise code, so this is the useful bit
function ensureFilesystemIsSet(timeout) {
  var start = Date.now();
  return new Promise(waitForFS); // set the promise object within the ensureFilesystemIsSet object

  // waitForFS makes the decision whether the condition is met
  // or not met or the timeout has been exceeded which means
  // this promise will be rejected
  function waitForFS(resolve, reject) {
    if (window.FS) resolve(window.FS);
    else if (timeout && Date.now() - start >= timeout)
      reject(new Error("timeout"));
    else setTimeout(waitForFS.bind(this, resolve, reject), 30);
  }
}

function loadAsset(asset) {
  fetch(asset.url)
    .then((res) => res.blob()) // Gets the response and returns it as a blob
    .then((res) => res.arrayBuffer())
    .then((blob) => {
      var bytes = new Uint8Array(blob);

      let parentDirectory = asset.path;
      let fileName = asset.name;

      console.log("WASM: Creating directory '" + parentDirectory + "'");
      var pathRet = FS.createPath("/", parentDirectory, true, true);

      console.log(
        "WASM: Creating file '" +
          fileName +
          "' in directory '" +
          parentDirectory +
          "'"
      );
      if (!MONO.mono_wasm_load_data_archive(bytes, parentDirectory)) {
        var fileRet = FS.createDataFile(
          parentDirectory,
          fileName,
          bytes,
          true,
          true,
          true
        );
      }
    })
    .catch((e) => {
      console.log(e);
    });
}

// This runs the promise code
ensureFilesystemIsSet(timeout_ms)
  .then(() => {
    assets.forEach((asset) => loadAsset(asset));
  })
  .catch((e) => {
    console.log(e);
  });
