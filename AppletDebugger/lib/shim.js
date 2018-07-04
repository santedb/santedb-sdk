// SanteDB Self-Hosted SHIM

SanteDBApplicationService.GetStatus = function () {
    return '[ "Dummy Status", 0 ]';
}

SanteDBApplicationService.ShowToast = function (string) {
    console.info("TOAST: " + string);
}

SanteDBApplicationService.GetOnlineState = function () {
    return true;
}


SanteDBApplicationService.IsAdminAvailable = function () {
    return true;
}

SanteDBApplicationService.IsClinicalAvailable = function () {
    return true;
}


SanteDBApplicationService.BarcodeScan = function () {
    return SanteDBApplicationService.NewGuid().substring(0, 8);
}

SanteDBApplicationService.Close = function () {
    alert("You need to restart the MiniIMS service for the changes to take effect");
    window.close();
}

SanteDBApplicationService.GetLocale = function () {
    return (navigator.language || navigator.userLanguage).substring(0, 2);
}

SanteDBApplicationService.NewGuid = function () {
    var retVal = "";
    $.ajax({
        url: "/__app/uuid",
        success: function (data) { retVal = data; },
        async: false
    });
    return retVal;
}

SanteDBApplicationService.GetMagic = function () {
    return SanteDBModel.EmptyGuid;
}