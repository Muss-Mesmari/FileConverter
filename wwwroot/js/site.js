
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
function ShowDownloadingByType() {

    var typeInputOne = document.getElementById("downloading-type-option-One");
    var typeInputTwo = document.getElementById("downloading-type-option-Two");
    var typeInputThree = document.getElementById("downloading-type-option-Three");
    var typeInputFour = document.getElementById("downloading-type-option-Four");
    var typeInputFive = document.getElementById("downloading-type-option-Five");
    var typeInputSix = document.getElementById("downloading-type-option-Six");
    var tableInput = document.getElementById("downloading-table-option");



    if (typeInputOne.style.display === "none") {
        typeInputOne.style.display = "block";
        typeInputTwo.style.display = "block";
        typeInputThree.style.display = "block";
        typeInputFour.style.display = "block";
        typeInputFive.style.display = "block";
        typeInputSix.style.display = "block";
        tableInput.style.display = "none";
    }
}

function ShowDownloadingByTable() {

    var typeInputOne = document.getElementById("downloading-type-option-One");
    var typeInputTwo = document.getElementById("downloading-type-option-Two");
    var typeInputThree = document.getElementById("downloading-type-option-Three");
    var typeInputFour = document.getElementById("downloading-type-option-Four");
    var typeInputFive = document.getElementById("downloading-type-option-Five");
    var typeInputSix = document.getElementById("downloading-type-option-Six");
    var tableInput = document.getElementById("downloading-table-option");

    if (tableInput.style.display === "none") {
        tableInput.style.display = "block";
        typeInputOne.style.display = "none";
        typeInputTwo.style.display = "none";
        typeInputThree.style.display = "none";
        typeInputFour.style.display = "none";
        typeInputFive.style.display = "none";
        typeInputSix.style.display = "none";
    }
}

function HighlightDownloadingByTypeTitle() {

    if (!document.getElementById("download-by-type-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-type-btn").className += ' active font-weight-normal';
    }

    if (document.getElementById("download-by-table-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-table-btn").className =
            document.getElementById("download-by-table-btn").className.replace
                (/(?:^|\s)active font-weight-normal(?!\S)/g, '');
    }
}

function HighlightDownloadingByTableTitle() {

    if (!document.getElementById("download-by-table-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-table-btn").className += ' active font-weight-normal';
    }

    if (document.getElementById("download-by-type-btn").className.match(/(?:^|\s)active(?!\S)/)) {
        document.getElementById("download-by-type-btn").className =
            document.getElementById("download-by-type-btn").className.replace
                (/(?:^|\s)active font-weight-normal(?!\S)/g, '')
    }
}


function ActivateDownloadingByTypeOption() {
    ShowDownloadingByType();
    HighlightDownloadingByTypeTitle();
}
function ActivateDownloadingByTableOption() {
    ShowDownloadingByTable();
    HighlightDownloadingByTableTitle();  
}
/* -------------- */