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
