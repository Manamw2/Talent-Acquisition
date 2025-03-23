var dataTable;
$(document).ready(() => loadTableData());

function loadTableData() {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            url: '/api/HiringTemplateApi',  // Match the controller name exactly (case-sensitive)
            dataSrc: '',
            error: function (xhr, error, thrown) {
                console.error('DataTables Ajax error:', xhr.status, error, thrown);
                console.log('Response text:', xhr.responseText);
            }
        },
        "columns": [
            { data: 'name', "width": "25%" },
            {
                data: 'createdOn',
                "width": "10%",
                "render": function (data) {
                    // Format date properly
                    return data ? new Date(data).toLocaleDateString() : '';
                }
            },
            { data: 'addedByName', "width": "25%" },
            {
                data: 'id',
                "width": "25%",
                "render": function (data) {
                    return `<div class="input-group" role="group">
                            <a href="/HiringTemplate/Details?id=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Details
                            </a>
                            <a onclick="deleteTemplate('/api/HiringTemplateApi/${data}')" class="btn btn-danger mx-2">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>`
                }
            }]
    });
}


function deleteTemplate(url) {
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
                    toastr.success("Hiring Template was deleted successfully");
                },
                error: function (xhr, status, error) {
                    console.error("Delete failed:", error);
                    toastr.error("Failed to delete hiring Template. Please try again.");
                }
            });
        }
    });
}