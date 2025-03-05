// Add a custom sorting function
$.fn.dataTable.ext.type.detect.unshift(function (data) {
    if (typeof data === 'string' && data.includes('failed-files-wrapper')) {
        return 'failed-files-count';
    }
    return null;
});

$.fn.dataTable.ext.type.order['failed-files-count-asc'] = function (a, b) {
    const aCount = $(a).find('.badge').first().text() || 0;
    const bCount = $(b).find('.badge').first().text() || 0;
    return aCount - bCount;
};

$.fn.dataTable.ext.type.order['failed-files-count-desc'] = function (a, b) {
    const aCount = $(a).find('.badge').first().text() || 0;
    const bCount = $(b).find('.badge').first().text() || 0;
    return bCount - aCount;
};

function loadDataTable() {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": "/cvsbulkupload/getall",
            "type": "GET",
            "error": function (xhr, error, thrown) {
                console.log("Error with AJAX request:", error);
                console.log("Server response:", xhr.responseText);
            }
        },
        "scrollX": true, // Enable horizontal scroll
        "columns": [
            { "data": "startedBy" },
            {
                "data": "startDate",
                "type": "date", // Use the datetime type
                "render": function (data) {
                    return new Date(data).toLocaleString();
                }
            },
            {
                "data": "endDate",
                "type": "date", // Use the datetime type
                "render": function (data) {
                    return data ? new Date(data).toLocaleString() : 'N/A';
                }
            },
            { "data": "totalFiles" },
            { "data": "successfulFiles" },
            {
                "data": "failedFiles",
                "type": "failed-files-count",
                "render": function (data, type, row) {
                    return `
            <div class="failed-files-wrapper" tabindex="0">
                ${data}
                <div class="failed-files-details">
                    <ul class="list-group list-group-flush">
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            CV Already Exists
                            <span class="badge bg-secondary rounded-pill">${row.cvExists}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            Email Already Exists
                            <span class="badge bg-secondary rounded-pill">${row.emailExists}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            Invalid Email
                            <span class="badge bg-secondary rounded-pill">${row.emailNotValid}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            Server Errors
                            <span class="badge bg-secondary rounded-pill">${row.serverErrors}</span>
                        </li>
                    </ul>
                </div>
            </div>`;
                }
            },
            {
                "data": "isRunning",
                "render": function (data) {
                    return data ?
                        '<span class="badge bg-success">Running</span>' :
                        '<span class="badge bg-secondary">Completed</span>';
                }
            }
        ]
    });
}

// Add hover effect for failed files details
document.addEventListener('DOMContentLoaded', function () {
    $(document).on('mouseenter', '.failed-files-wrapper', function () {
        const details = $(this).find('.failed-files-details');
        details.fadeIn(200);

        // Adjust position if it overflows the viewport
        const rect = details[0].getBoundingClientRect();
        if (rect.right > window.innerWidth) {
            details.css('left', `-${rect.right - window.innerWidth + 10}px`);
        }
        if (rect.bottom > window.innerHeight) {
            details.css('top', `-${rect.bottom - window.innerHeight + 10}px`);
        }
    });

    $(document).on('mouseleave', '.failed-files-wrapper', function () {
        $(this).find('.failed-files-details').fadeOut(200);
    });
});

// Add some CSS styles to make the view better
const style = document.createElement('style');
style.innerHTML = `
.failed-files-wrapper {
    position: relative;
    display: inline-block;
    cursor: pointer;
}

.failed-files-details {
    display: none;
    position: absolute;
    top: 100%;
    left: 50%;
    transform: translateX(-50%);
    z-index: 1000; /* Ensure it appears above other elements */
    width: 200px;
    background: #fff;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.15);
    border-radius: 8px;
    padding: 10px;
    margin-top: 5px; /* Add some space between the text and the popup */
}

/* Show on hover */
.failed-files-wrapper:hover .failed-files-details {
    display: block;
}
`;
document.head.appendChild(style);

// Update the table headers
$(document).ready(function () {
    $('#myTable thead tr').html(`
        <th>Started By</th>
        <th>Start Date</th>
        <th>End Date</th>
        <th>Total Files</th>
        <th>Successful Files</th>
        <th>Failed Files</th>
        <th>Status</th>
    `);

    loadDataTable();
});
