$(document).ready(function () {
    $('#SubmitBtn1').on('click', function () {
        Swal.fire({
            title: 'Saved',
            icon: 'success',
            allowOutsideClick: () => {
                const popup = Swal.getPopup()
                popup.classList.remove('swal2-show')
                setTimeout(() => {
                    popup.classList.add('animate__animated', 'animate__headShake')
                })
                setTimeout(() => {
                    popup.classList.remove('animate__animated', 'animate__headShake')
                }, 500)
                return false
            }
        })
    })
});


$(document).ready(function () {
    $(".multiple-select2").select2({ placeholder: "--Select Person--" });
    $("#EmpName").autocomplete({
        source: ["Dheerendra Kumar Joshi", "Akash Dilip Kumbhar"]
    });
    $("#Reasons").autocomplete({
        source: ["Personal Reason", "Medical Issue", "Technical Issue", "Medical Issue"]
    });
});