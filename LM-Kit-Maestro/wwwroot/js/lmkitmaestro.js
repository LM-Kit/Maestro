// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

function getScrollHeight () {
    const element = document.getElementById("message-list");

    return element.scrollHeight;
};

function getViewHeight () {
    return window.innerHeight;
};

function resizeUserInput() {
    const element = document.getElementById("chat-box");

    // this.style.height = ""; this.style.height = this.scrollHeight + "px"
    element.style.height = "";
    element.style.height = element.scrollHeight + "px";
}

function setUserInputFocus() {
    const element = document.getElementById("chat-box");

    element.focus();
}