var dataTable;
$(document).ready(() => loadTableData());

function loadTableData() {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            url: '/api/HiringStageApi',  // Match the controller name exactly (case-sensitive)
            dataSrc: '',
            error: function (xhr, error, thrown) {
                console.error('DataTables Ajax error:', xhr.status, error, thrown);
                console.log('Response text:', xhr.responseText);
            }
        },
        "columns": [
            { data: 'stageName', "width": "25%" },
            {
                data: 'outcomeType',
                "width": "15%",
                "render": function (data) {
                    // Map the enum integer value to the correct string
                    const outcomeTypes = ["PassFail", "Set", "Evaluation"];
                    return outcomeTypes[data] || "Unknown";
                }
            },
            {
                data: 'createdOn',
                "width": "10%",
                "render": function (data) {
                    // Format date properly
                    return data ? new Date(data).toLocaleDateString() : '';
                }
            },
            { data: 'createdBy', "width": "25%" },
            {
                data: 'id',
                "width": "25%",
                "render": function (data) {
                    return `<div class="input-group" role="group">
                            <a href="/HiringStage/Details?id=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Details
                            </a>
                            <a onclick="deleteStage('/api/HiringStageApi/${data}')" class="btn btn-danger mx-2">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>`
                }
            }]
    });
}

function deleteStage(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success("Hiring stage was deleted successfully");
                },
                error: function (xhr, status, error) {
                    console.error("Delete failed:", error);
                    toastr.error("Failed to delete hiring stage. Please try again.");
                }
            });
        }
    });
}