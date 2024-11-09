// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

function getScrollHeight () {
    const element = document.getElementById("message-list");

    return element.scrollHeight;
};

function getViewHeight () {
    return window.innerHeight;
};