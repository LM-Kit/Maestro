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


function scrollToEnd(smooth) {
    const element = document.getElementById('conversation-content');

    if (element != null) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: smooth ? 'smooth' : 'auto'
        });
    }
}

/*
    ChatInput 
*/
function resizeUserInput() {
    const inputText = document.getElementById('input-text');
    const inputBorder = document.getElementById('input-textarea-wrapper');

    inputText.style.height = "auto";
    inputText.style.height = `${inputText.scrollHeight}px`;

    var lineCount = countLines(inputText);

    if (lineCount > 1) {
        inputBorder.classList.add('input-textarea-multiline');
        inputBorder.classList.remove('input-textarea-initial');

    } else {
        inputBorder.classList.remove('input-textarea-multiline');
        inputBorder.classList.add('input-textarea-initial');
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

/* ChatSidebar */
let dotnetRef = null;

window.attachResizeListeners = (dotnetReference) => {
    dotnetRef = dotnetReference; // Store reference globally
    document.addEventListener("mousemove", handleMouseMove);
    document.addEventListener("mouseup", handleMouseUp);
};

window.detachResizeListeners = () => {
    document.removeEventListener("mousemove", handleMouseMove);
    document.removeEventListener("mouseup", handleMouseUp);
    dotnetRef = null;
};

function handleMouseMove(event) {
    if (dotnetRef) {
        dotnetRef.invokeMethodAsync('OnMouseMove', { clientX: event.clientX });
    }
}

function handleMouseUp() {
    if (dotnetRef) {
        dotnetRef.invokeMethodAsync('OnMouseUp');
    }
    window.detachResizeListeners();
}

// Theme management
window.applyTheme = function(primary, accent) {
    const props = {
        '--Primary': primary,
        '--PrimaryAccent': accent,
        '--PrimaryMuted': primary + '26',
        '--Secondary': primary
    };
    
    const root = document.documentElement;
    const darkEl = document.querySelector('.dark');
    
    for (const [key, val] of Object.entries(props)) {
        root.style.setProperty(key, val);
        if (darkEl) darkEl.style.setProperty(key, val);
    }
};
