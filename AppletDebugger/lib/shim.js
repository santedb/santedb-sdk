// SanteDB Self-Hosted SHIM

__SanteDBAppService.GetStatus = function () {
    return '[ "Dummy Status", 0 ]';
}

__SanteDBAppService.ShowToast = function (string) {
    console.info("TOAST: " + string);
}

__SanteDBAppService.GetOnlineState = function () {
    return true;
}


__SanteDBAppService.IsAdminAvailable = function () {
    return true;
}

__SanteDBAppService.IsClinicalAvailable = function () {
    return true;
}


__SanteDBAppService.BarcodeScan = function () {
    return SanteDBApplicationService.NewGuid().substring(0, 8);
}

__SanteDBAppService.Close = function () {
    alert("You need to restart the service for the changes to take effect");
    window.close();
}

__SanteDBAppService.GetLocale = function () {
    return (navigator.language || navigator.userLanguage).substring(0, 2);
}

__SanteDBAppService.NewGuid = function () {
    var retVal = "";
    $.ajax({
        url: "/__app/uuid",
        success: function (data) { retVal = data; },
        async: false
    });
    return retVal;
}

__SanteDBAppService.GetMagic = function () {
    return SanteDBModel.EmptyGuid;
}