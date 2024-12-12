// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

function getScrollHeight() {
    const element = document.getElementById("message-list");

    return element.scrollHeight;
};

function getViewHeight() {
    return window.innerHeight;
};

function resizeUserInput() {
    const element = document.getElementById("chat-box");

    element.style.height = "";
    element.style.height = element.scrollHeight + "px";

    const lineHeight = 28;
    var lineCount = element.scrollHeight / lineHeight;

    // If the height exceeds max-height (200px), the scrollbar should appear.
    if (element.scrollHeight > 200) {
        element.style.height = "200px"; // Limit height to 200px
    }
    else {
        // Adding top and bottom margin depending on number of lines, to account for border radius of input box when showing scrollbar.
        if (lineCount > 2) {
            element.style.marginBottom = "16px";
            element.style.marginTop = "16px";
        }
        else if (lineCount > 1) {
            element.style.marginTop = "8px";
            element.style.marginBottom = "8px";
        }
        else {
            element.style.marginBottom = "0px";
            element.style.marginTop = "0px";
        }
    }
}

function setUserInputFocus() {
    const element = document.getElementById("chat-box");

    element.focus();
}