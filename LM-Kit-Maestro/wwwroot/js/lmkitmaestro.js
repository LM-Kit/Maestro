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



/* 
    Chat 
*/
function initializeScrollHandler(dotNetHelper) {
    const container = document.getElementById('conversation-container');
    container.addEventListener('scroll', () => {
        dotNetHelper.invokeMethodAsync('OnConversationContainerScrolled', container.scrollTop);
    });
}

function getScrollHeight() {
    const element = document.getElementById('conversation-container');
    return element.scrollHeight;
};

function getConversationViewHeight() {
    const element = document.getElementById('conversation-container');
    return element.clientHeight;
};

function setUserInputFocus() {
    const element = document.getElementById('input-text');

    element.focus();
}

function scrollToEnd(smooth) {
    const container = document.getElementById('conversation-container');
    container.scrollTo({
        top: container.scrollHeight,
        behavior: smooth ? 'smooth' : 'auto'
    });
}


/*
    UserInput 
*/
//document.getElementById('input-text').addEventListener('keydown', function (e) {
//    if (e.key == 'Enter' && !e.shiftKey) {
//        // prevent default behavior
//        e.preventDefault();
//        return false;
//    }
//}, false);

function resizeUserInput() {
    const chatBox = document.getElementById('input-text');
    const chatBorder = document.getElementById('input-border');


    chatBox.style.height = "";
    chatBox.style.height = chatBox.scrollHeight + "px";

    var style = window.getComputedStyle(chatBox);
    var lineHeight = parseFloat(style.getPropertyValue('line-height'));
    var lineCount = Math.round(chatBox.scrollHeight / lineHeight);

    // If the height exceeds max-height (200px), the scrollbar should appear.
    if (chatBox.scrollHeight > 200) {
        chatBox.style.height = "200px"; // Limit height to 200px
    }

    if (lineCount > 1) {
        chatBox.classList.add('input-text-large');
        chatBorder.classList.add('input-border-large');
        chatBox.classList.remove('input-text-small');
        chatBorder.classList.remove('input-border-small');
    } else {
        chatBox.classList.add('input-text-small');
        chatBorder.classList.add('input-border-small');
        chatBox.classList.remove('input-text-large');
        chatBorder.classList.remove('input-border-large');
    }
}
