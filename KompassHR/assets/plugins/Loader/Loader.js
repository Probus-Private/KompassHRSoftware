//$(window).on('load', function () {
//    setTimeout(removeLoader, 100);
//});
function removeLoader() {
    debugger
    $(".loaderBX").fadeOut(100, function () {
        $(".loaderBX").remove();
        document.body.classList.remove('no-scroll');
    });
}

function addLoader() {
    debugger
    $(".loaderBX").fadeIn(100, function () {
        $(".loaderBX").add();
        document.body.classList.add('no-scroll');
    });
}