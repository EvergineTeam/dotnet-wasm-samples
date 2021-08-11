var Module = typeof Module !== "undefined" ? Module : {};
let App = {
  mainCanvasId: undefined,
  configure: function (canvasId, assemblyName, className) {
    this.mainCanvasId = canvasId;
    this.Program.assemblyName = assemblyName;
    this.Program.className = className;
  },
  init: function () {
    let canvas = document.getElementById(this.mainCanvasId);
    if (!canvas) {
      alert("Initialization failed: WebGL canvas element not found.");
    }

    // As a default initial behavior, pop up an alert when webgl context is lost. To make your
    // application robust, you may want to override this behavior before shipping!
    // See http://www.khronos.org/registry/webgl/specs/latest/1.0/#5.15.2
    canvas.addEventListener(
      "webglcontextlost",
      function (e) {
        alert("WebGL context lost. You will need to reload the page.");
        e.preventDefault();
      },
      false
    );

    Module.canvas = canvas;

    this.updateCanvasSize();
    // // this.Program.Main(this.mainCanvasId);
    window.addEventListener("resize", this.resizeAppSize.bind(this));
  },
  enterImmersive: function (mode) {
    this.Program.EnterImmersive(mode, this.mainCanvasId);
  },
  exitImmersive: function () {
    this.Program.ExitImmersive();
  },
  resizeAppSize: function () {
    this.updateCanvasSize();
    this.Program.UpdateCanvasSize(this.mainCanvasId);
  },
  updateCanvasSize: function () {
    let devicePixelRatio = window.devicePixelRatio || 1;
    Module.canvas.style.width = window.innerWidth + "px";
    Module.canvas.style.height = window.innerHeight + "px";
    Module.canvas.width = window.innerWidth * devicePixelRatio;
    Module.canvas.height = window.innerHeight * devicePixelRatio;
  },
  Program: {
    assemblyName: undefined,
    className: undefined,
    // // Main: function (canvasId) {
    // //   this.invoke("Main", [canvasId]);
    // // },
    EnterImmersive: function (mode, canvasId) {
      this.invoke("EnterImmersive", [mode, canvasId]);
    },
    ExitImmersive: function () {
      this.invoke("ExitImmersive");
    },
    invoke: function (methodName, args) {
      BINDING.call_static_method(
        `[${this.assemblyName}] ${this.className}:${methodName}`,
        args
      );
    },
  },
};
