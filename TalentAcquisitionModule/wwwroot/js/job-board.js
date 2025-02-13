//$(document).ready(function () {
//    // Handle click event on job cards
//    $('.job-card').off('click').on('click', function (e) {
//        e.preventDefault();

//        // Remove the 'selected-job' class from all job cards
//        $('.job-card').removeClass('selected-job');
//        // Add the 'selected-job' class to the clicked job card
//        $(this).addClass('selected-job');

//        // Get job details from data attributes
//        const jobId = $(this).data('job-id');
//        const jobTitle = $(this).data('job-title');
//        const jobDescription = $(this).data('job-description');
//        const jobApplicationCount = $(this).data('job-application-count');
//        const jobType = $(this).data('job-type');

//        // Update the middle section with the clicked job's details
//        $('#job-title').text(jobTitle);

//        // Clear and update the job description content only
//        $('#description-content').empty().html(jobDescription);

//        // Update the badges
//        $('.badge.bg-info').text(`${jobApplicationCount} Applications`);
//        $('.badge.bg-secondary').text(jobType);

//        // Update the Apply button's data-job-id attribute
//        $('#applyButton').attr('data-job-id', jobId);

//        console.log('Updated job description for job:', jobId); // Debug log
//    });

//    // Handle job type filtering
//    $('#job-type-filter').off('change').on('change', function () {
//        const selectedJobType = $(this).val().toLowerCase();
//        $('.job-card').each(function () {
//            const jobType = $(this).data('job-type').toLowerCase();
//            $(this).toggle(selectedJobType === 'all' || jobType === selectedJobType);
//        });
//    });
//});