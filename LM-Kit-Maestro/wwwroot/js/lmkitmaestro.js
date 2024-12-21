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

