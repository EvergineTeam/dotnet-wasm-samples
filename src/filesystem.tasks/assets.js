var timeout_ms = 5 * 1000;
// // var assets = [{ url: "assets/file.txt", path: "assets", name: "file.txt" }];

var assetsLength = assets.length;
var progressSum = 0;
var assetsProgress = new Object();
var assetsLoaded = 0;

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
  assetsProgress[asset.url] = 0;

  let xhr = new XMLHttpRequest();
  xhr.open("GET", asset.url, true);
  xhr.responseType = "blob";
  xhr.onprogress = function (event) {
    progressSum -= assetsProgress[asset.url];
    assetsProgress[asset.url] = event.loaded / event.total;
    progressSum += assetsProgress[asset.url];
    if (Module["setProgress"])
      Module["setProgress"]((progressSum / assetsLength) * 100);
    console.log("Assets progress: " + (progressSum / assetsLength) * 100 + "%");
  };
  xhr.onerror = function (event) {
    throw new Error("NetworkError for: " + packageName);
  };
  xhr.onload = function (event) {
    if (
      xhr.status == 200 ||
      xhr.status == 304 ||
      xhr.status == 206 ||
      (xhr.status == 0 && xhr.response)
    ) {
      // file URLs can return 0
      var packageData = xhr.response;
      packageData
        .arrayBuffer()
        .then((data) => saveFile(asset.path, asset.name, data));
    } else {
      throw new Error(xhr.statusText + " : " + xhr.responseURL);
    }
  };
  xhr.send(null);
}

function saveFile(parentDirectory, fileName, data) {
  let bytes = new Uint8Array(data);

  console.log("WASM: Creating directory '" + parentDirectory + "'");
  let pathRet = FS.createPath("/", parentDirectory, true, true);

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
  assetsLoaded += 1;
}

function areAllAssetsLoaded() {
  return assetsLength == assetsLoaded;
}

// This runs the promise code
ensureFilesystemIsSet(timeout_ms)
  .then(() => {
    assets.forEach((asset) => loadAsset(asset));
  })
  .catch((e) => {
    console.log(e);
  });
