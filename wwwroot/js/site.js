
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
