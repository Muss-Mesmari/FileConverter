
// Hover Card effect //
$(document).ready(function () {
    $(".task-card").hover(
        function () {
            $(this).addClass('bg-dark').css('cursor', 'pointer');
        }, function () {
            $(this).removeClass('bg-dark');
        }
    );
});
/* -------------- */

// Downloading options //
function disable() {

    var type = document.getElementById("downloading-type-option");
    var table = document.getElementById("downloading-table-option");

    if (type.style.display === "none")
    {
        type.style.display = "block";
        table.style.display = "none";
    }


    if (!document.getElementById("download-by-type-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-type-btn").className += ' active font-weight-normal';
    }

    if (document.getElementById("download-by-table-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-table-btn").className =
            document.getElementById("download-by-table-btn").className.replace
            (/(?:^|\s)active font-weight-normal(?!\S)/g, '');
    }

}
function enable() {

    var type = document.getElementById("downloading-type-option");
    var table = document.getElementById("downloading-table-option");

    if (table.style.display === "none") {
        table.style.display = "block";
        type.style.display = "none";
    }


    if (!document.getElementById("download-by-table-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-table-btn").className += ' active font-weight-normal';
    }

    if (document.getElementById("download-by-type-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-type-btn").className =
            document.getElementById("download-by-type-btn").className.replace
            (/(?:^|\s)active font-weight-normal(?!\S)/g, '')
    }
}
/* -------------- */