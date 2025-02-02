// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

document.addEventListener("DOMContentLoaded", function () {
    const platform = window.navigator.userAgent;
    let platformClass = "";

    if (platform.includes("Windows")) {
        platformClass = "windows";
    } else if (platform.includes("Macintosh")) {
        platformClass = "mac";
    }

    if (platformClass) {
        document.body.classList.add(platformClass);
    }
});

function debounce(func, delay) {
    let timeout;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), delay);
    };
}

/* 
    Chat 
*/
function initializeScrollHandler(dotNetHelper) {
    const container = document.getElementById('conversation-content');
    container.addEventListener('scroll', () => {
        dotNetHelper.invokeMethodAsync('OnChatScrolled', container.scrollTop);
    });
}

function getScrollHeight() {
    const element = document.getElementById('conversation-content');
    return element.scrollHeight;
};

function getConversationViewHeight() {
    const element = document.getElementById('conversation-content');
    return element.clientHeight;
};

function setUserInputFocus() {
    const element = document.getElementById('conversation-content');

    element.focus();
}

function scrollToEnd(smooth) {
    const element = document.getElementById('conversation-content');

    if (element != null) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: smooth ? 'smooth' : 'auto'
        });
    }
}

/* ChatSidebar */
function initSidebarResizeHandler(dotnetReference, sidebarContainer, position) {
    var rect = sidebarContainer.getBoundingClientRect();
    let isResizing = false;
    var resizeHandle = sidebarContainer.getElementsByClassName('resize-handle')[0];

    resizeHandle.addEventListener("mousedown", function (e) {
        isResizing = true;
        document.addEventListener("mousemove", debouncedOnMouseMove);
        document.addEventListener("mouseup", onMouseUp);
    });

    const debouncedOnMouseMove = debounce(onMouseMove, 10);

    async function onMouseMove(e) {
        if (!isResizing) return;

        var rect = resizeHandle.getBoundingClientRect();
        var sidebarWidth = sidebarContainer.offsetWidth;
        var isToggled = await dotnetReference.invokeMethodAsync('CheckIsToggled');

        let newWidth;
        console.error(isToggled + "  " + rect.left); 
       
        if (position == "Right") {

            if (!isToggled) {

                console.error("not toggled");
                console.error(sidebarContainer.offsetWidth);
            }
            else {
                if (e.clientX > rect.left) {
                    newWidth = Math.max(150, sidebarWidth - (e.clientX - rect.left));

                    if (sidebarWidth - (e.clientX - rect.left) < 150) {
                        dotnetReference.invokeMethodAsync('ToggleSidebar', false);
                    }
                } else {
                    console.error(rect.left - e.clientX);

                    if (!isToggled && rect.left - e.clientX > 70) {
                        console.error("------------------------------toggle");
                        dotnetReference.invokeMethodAsync('ToggleSidebar', true);
                    }

                    newWidth = Math.min(500, sidebarWidth + (rect.left - e.clientX));
                }
            }
        } else {
            if (e.clientX < rect.right) {
                newWidth = Math.max(150, sidebarWidth - (rect.right - e.clientX));
            } else {
                newWidth = Math.min(500, sidebarWidth + (e.clientX - rect.right));
            }
        }

        sidebarContainer.style.width = newWidth + "px";
    }

    function onMouseUp() {
        console.error("mouse up");

        isResizing = false;
        document.removeEventListener("mousemove", debouncedOnMouseMove);
        document.removeEventListener("mouseup", onMouseUp);
    }
}


/*
    UserInput 
*/
function resizeUserInput() {
    const inputText = document.getElementById('input-text');
    const inputBorder = document.getElementById('input-border');

    inputText.style.height = "auto";
    inputText.style.height = `${inputText.scrollHeight}px`;

    var lineCount = countLines(inputText);

    if (lineCount > 1) {
        inputBorder.classList.add('input-border-large');
        inputBorder.classList.remove('input-border-small');
    } else {
        inputBorder.classList.add('input-border-small');
        inputBorder.classList.remove('input-border-large');
    }
}

/** @type {HTMLTextAreaElement} */
var _buffer;

/**
* Returns the number of lines in a textarea, including wrapped lines.
*
* __NOTE__:
* [textarea] should have an integer line height to avoid rounding errors.
*/
function countLines(textarea) {
    if (_buffer == null) {
        _buffer = document.createElement('textarea');
        _buffer.style.border = 'none';
        _buffer.style.height = '0';
        _buffer.style.overflow = 'hidden';
        _buffer.style.padding = '0';
        _buffer.style.position = 'absolute';
        _buffer.style.left = '0';
        _buffer.style.top = '0';
        _buffer.style.zIndex = '-1';
        document.body.appendChild(_buffer);
    }

    var cs = window.getComputedStyle(textarea);
    var pl = parseInt(cs.paddingLeft);
    var pr = parseInt(cs.paddingRight);
    var lh = parseInt(cs.lineHeight);

    // [cs.lineHeight] may return 'normal', which means line height = font size.
    if (isNaN(lh)) lh = parseInt(cs.fontSize);

    // Copy content width.
    _buffer.style.width = (textarea.clientWidth - pl - pr) + 'px';

    // Copy text properties.
    _buffer.style.font = cs.font;
    _buffer.style.letterSpacing = cs.letterSpacing;
    _buffer.style.whiteSpace = cs.whiteSpace;
    _buffer.style.wordBreak = cs.wordBreak;
    _buffer.style.wordSpacing = cs.wordSpacing;
    _buffer.style.wordWrap = cs.wordWrap;

    // Copy value.
    _buffer.value = textarea.value;

    var result = Math.floor(_buffer.scrollHeight / lh);
    if (result == 0) result = 1;
    return result;
}