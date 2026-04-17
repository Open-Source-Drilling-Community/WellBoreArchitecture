window.wellBoreArchitecture = window.wellBoreArchitecture || {};

window.wellBoreArchitecture.blurActiveElement = function () {
    const activeElement = document.activeElement;
    if (activeElement && typeof activeElement.blur === "function") {
        activeElement.blur();
    }
};
