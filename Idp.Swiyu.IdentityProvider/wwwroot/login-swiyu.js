
$(document).ready(function () {
    //dom is ready

    const form = document.getElementById("pollSwiyu");
    const buttonPollSwiyu = document.getElementById("buttonPollSwiyu");

    function pollSwiyuSubmit(e) {
        e.preventDefault();

        if (form) {
            form.submit();
        }
 
        return false;
    }

    if (buttonPollSwiyu) {
        buttonPollSwiyu.onclick = pollSwiyuSubmit;
    }
});
