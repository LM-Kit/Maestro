// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

function getScrollHeight () {
    const element = document.getElementById("message-list");

    return element.scrollHeight;
};

function getViewHeight () {
    return window.innerHeight;
};

var getHtmlElement = function () {
    return document.getElementsByTagName("html")[0];
}

var addCssClassToHtml = function (cssClass) {
    var html = getHtmlElement();

    if (!html.classList.contains(cssClass)) {
        html.classList.add(cssClass);
    }
}
