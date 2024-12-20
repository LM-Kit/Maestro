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

function resizeUserInput() {
    const chatBox = document.getElementById('chat-box');
    const chatBorder = document.getElementById('chat-border');


    chatBox.style.height = "";
    chatBox.style.height = chatBox.scrollHeight + "px";

    var style = window.getComputedStyle(chatBox);
    var lineHeight = parseFloat(style.getPropertyValue('line-height'));
    var lineCount = Math.round(chatBox.scrollHeight / lineHeight);

    // If the height exceeds max-height (200px), the scrollbar should appear.
    if (chatBox.scrollHeight > 200) {
        chatBox.style.height = "200px"; // Limit height to 200px
    } else {
        // Adding top and bottom margin depending on number of lines, to account for border radius of input box when showing scrollbar.
        if (lineCount > 2) {
            chatBox.style.marginBottom = "16px";
            chatBox.style.marginTop = "16px";
        } else if (lineCount > 1) {
            chatBox.style.marginTop = "8px";
            chatBox.style.marginBottom = "8px";

        } else {
            chatBox.style.marginBottom = "0px";
            chatBox.style.marginTop = "0px";
        }
    }


    if (lineCount > 1) {
        chatBorder.classList.add('chat-border-large');
        chatBorder.classList.remove('chat-border-small');
    } else {
        chatBorder.classList.add('chat-border-small');
        chatBorder.classList.remove('chat-border-large');
    }
}

function setUserInputFocus() {
    const element = document.getElementById('chat-box');

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
document.getElementById('chat-box').addEventListener('keydown', function (e) {
    if (e.key == 'Enter' && !e.shiftKey) {
        // prevent default behavior
        e.preventDefault();
        return false;
    }
}, false);