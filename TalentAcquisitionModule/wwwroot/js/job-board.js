$(document).ready(function () {
    // Handle click event on job cards
    $('.job-card').on('click', function (e) {
        e.preventDefault(); // Prevent default link behavior

        // Get job details from data attributes
        const jobId = $(this).data('job-id');
        const jobTitle = $(this).data('job-title');
        const jobLocation = $(this).data('job-location');
        const jobDescription = $(this).data('job-description');
        const jobApplicationCount = $(this).data('job-application-count');
        const jobType = $(this).data('job-type');

        // Update the middle section with the clicked job's details
        $('#job-title').text(jobTitle);
        $('#job-location').text(`📍 ${jobLocation}`);
        $('#job-description p').html(jobDescription);
        $('#job-description .badge.bg-info').text(`${jobApplicationCount} Applications`);
        $('#job-description .badge.bg-secondary').text(jobType);

        // Update the Apply button's data-job-id attribute
        $('#applyButton').attr('data-job-id', jobId);
    });

    // Handle job type filtering
    $('#job-type-filter').on('change', function () {
        const selectedJobType = $(this).val().toLowerCase(); // Get the selected job type

        // Loop through all job cards
        $('.job-card').each(function () {
            const jobType = $(this).data('job-type').toLowerCase(); // Get the job type of the card

            // Show or hide the job card based on the selected filter
            if (selectedJobType === 'all' || jobType === selectedJobType) {
                $(this).show(); // Show the job card
            } else {
                $(this).hide(); // Hide the job card
            }
        });
    });
});

window.addEventListener("scroll", function () {
    let navbar = document.getElementById("navbar");
    if (window.scrollY > 50) {
        navbar.classList.add("bg-dark");
    } else {
    }
}); 




