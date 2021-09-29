// Helper functions
function _getGlobalObject() {
    return window;
}

function _getObjectProperty(obj, property) {
    return obj[property];
}

function _setObjectProperty(obj, property, value) {
    obj[property] = value;
}

function _addSimpleEventListener(
    src,
    eventName,
    target,
    listenerName,
    options
) {
    src.addEventListener(
        eventName,
        (e) => {
            //let eref = DotNet.createJSObjectReference(e);
            target.invokeMethod(listenerName, e.type);
            //DotNet.disposeJSObjectReference(eref);
        },
        options
    );
}

function _addEventListener(
    src,
    eventName,
    target,
    listenerName,
    options
) {
    src.addEventListener(
        eventName,
        (e) => {
            let eref = DotNet.createJSObjectReference(e);
            target.invokeMethod(listenerName, e.type, eref);
            DotNet.disposeJSObjectReference(eref);
        },
        options
    );
}

function _removeEventListener(src, eventName, options) {
    src.addEventListener(eventName, null, options);
}