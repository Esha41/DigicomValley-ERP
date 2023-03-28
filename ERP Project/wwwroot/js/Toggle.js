function ChangeShiftStatus(id)
{
    $.ajax({
        type: "Get",
        url: '/Shifts/ChangeShiftStatus',
        data: { id: id },
        success: function (json) {
            if (json == "Success") {
                //swal("Updated!", "Customer Status Updated!", "success");
                window.location.reload();
            }


            // swal("Updated!", "Customer Status Updated!", "success");
            //window.location.reload();
        },
        error: function (xhr) {
            //  swal("Error", "Please Try again later!", "error");
        }
    });
}

function ChangeShiftTimmingStatus(id)
{
    $.ajax({
        type: "Get",
        url: '/OfficialShifts/ChangeShiftTimmingStatus',
        data: { id: id },
        success: function (json) {
            if (json == "Success") {
                //swal("Updated!", "Customer Status Updated!", "success");
                window.location.reload();
            }


            // swal("Updated!", "Customer Status Updated!", "success");
            //window.location.reload();
        },
        error: function (xhr) {
            //  swal("Error", "Please Try again later!", "error");
        }
    });
}

function ChangeApplicationsStatus(id) {
    $.ajax({
        type: "Get",
        url: '/Applications/ChangeApplicationsStatus',
        data: { id: id },
        success: function (json) {
            if (json == "Success") {
                //swal("Updated!", "Customer Status Updated!", "success");
                window.location.reload();
            }


            // swal("Updated!", "Customer Status Updated!", "success");
            //window.location.reload();
        },
        error: function (xhr) {
            //  swal("Error", "Please Try again later!", "error");
        }
    });
}

function ChangeLeavesCategoriesStatus(id) {
    $.ajax({
        type: "Get",
        url: '/LeavesCategories/ChangeLeavesCategoriesStatus',
        data: { id: id },
        success: function (json) {
            if (json == "Success") {
                //swal("Updated!", "Customer Status Updated!", "success");
                window.location.reload();
            }


            // swal("Updated!", "Customer Status Updated!", "success");
            //window.location.reload();
        },
        error: function (xhr) {
            //  swal("Error", "Please Try again later!", "error");
        }
    });
}